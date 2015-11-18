var canvas_handle = document.getElementById("play_canvas");
var context = canvas_handle.getContext("2d");

var map = [[]];
var map_dirty = false;
var map_width = 200;
var map_height = 275;
var canvas_drag = false;
var canvas_drag_start = [0, 0];
var canvas_drag_curr = [0, 0]
var canvas_draw_offset_x = 0;
var canvas_draw_offset_y = 0;
var canvas_draw_zoom_factor = 1.0;
var canvas_cell_size = 2;

$(document).ready(function() { map = generateMap(map_width, map_height, 0.08, 600, 2); map_dirty = true; });
$("#play_canvas").mousedown(function(event) {
    canvas_drag = true;
    canvas_drag_start = [event.offsetX, event.offsetY];

    return false;
});
$("#play_canvas").mouseup(function() { canvas_drag = false; return false; });
$("#play_canvas").mousemove(function(event) {
    if(canvas_drag) {
        map_dirty = true;
    	canvas_drag_curr = [event.offsetX, event.offsetY];
    }
});
canvas_handle.addEventListener("mousewheel", function(event) {
    map_dirty = true;
    canvas_draw_zoom_factor += event.wheelDelta/(8.0*120.0);

    if(canvas_draw_zoom_factor < 0.5) {
        canvas_draw_zoom_factor = 0.5;
    } else if(canvas_draw_zoom_factor > 4.0) {
        canvas_draw_zoom_factor = 4.0;
    }

    return false;
}, false);
$("#regenerate_button").click(function() {
    map_dirty = true;
    canvas_draw_offset_x = 0;
	canvas_draw_offset_y = 0;
	canvas_draw_zoom_factor = 1.0;

    map = generateMap(map_width, map_height, 0.08, 600, 2);
});
$("#safety_display").click(function() { map_dirty = true; });
$("#visibility_display").click(function() { map_dirty = true; });
var draw_interval_id = setInterval(function() {
    if(map_dirty) {
        map_dirty = false;
        canvas_draw_offset_x += canvas_drag_curr[0] - canvas_drag_start[0];
        canvas_draw_offset_y += canvas_drag_curr[1] - canvas_drag_start[1];
        canvas_drag_start = canvas_drag_curr;

    	drawMap(map, [canvas_draw_offset_x, canvas_draw_offset_y], canvas_draw_zoom_factor, canvas_cell_size, $("#safety_display").is(":checked"), $("#visibility_display").is(":checked"));
    }
}, 40);

function generateMap(width, height, worker_rate, max_workers, smooth_count)
{
    var start_position = [Math.floor(width/2.0), Math.floor(height/2.0)];
    var map = initializeMap(width, height);
    map = digCaverns(map, start_position, worker_rate, max_workers);
    map = smoothMap(map, smooth_count);
    map = applySafetyZone(map, start_position, 15, 80);
    map = applyLOS(map, start_position);

    var exit = pickExit(map);
    map[exit[0]][exit[1]].type = 2;
    map = applySafetyZone(map, exit, 5, 80);

    map = spawnEnemies(map, 4 + Math.floor(12*Math.random()));

    var item_spawn_safety = 0.08 + Math.random();
    map = spawnDungeonItem(map, (item_spawn_safety > 1.0 ? 1.0 : item_spawn_safety), 8);
    map = spawnGoldAndMaterialDrops(map, 0.075 + (0.125*Math.random()), 0.08 + (0.1*Math.random() - 0.05));

    return map;
}

function spawnDungeonItem(map, required_safety, enemy_search_range)
{
    var safe_spawns = [];
    var enemy_spawn_limit = 10;
    var enemy_spawns = [];
    var enemy_spawns_count = [];

    // Find appropriate spawns.
    for(x = 1; x < map.length - 1; ++x) {
        for(y = 1; y < map[0].length - 1; ++y) {
            if(map[x][y].safety_normalized >= required_safety && !map[x][y].isSolid && map[x][y].type == 0) {
                var enemy_count = 0;
                safe_spawns.push([x, y]);

                for(enemy_x = x - enemy_search_range; enemy_x <= x + enemy_search_range; ++enemy_x) {
                    for(enemy_y = y - enemy_search_range; enemy_y <= y + enemy_search_range; ++enemy_y) {
                        if(enemy_x > 1 && enemy_x < map.length - 1 && enemy_y > 1 && enemy_y < map[0].length - 1 && map[enemy_x][enemy_y].type == 3) {
                            ++enemy_count;
                        }
                    }
                }

                if(enemy_count > 0) {
                    if(enemy_spawns.length < enemy_spawn_limit) {
                        enemy_spawns.push([x, y]);
                    	enemy_spawns_count.push(enemy_count);
                    } else {
                        var index_of_smallest = 0;
                        var count_of_smallest = enemy_spawns_count[0];

                        for(index = 1; index < enemy_spawns.length; ++index) {
                            if(enemy_spawns_count[index] < count_of_smallest) {
                                index_of_smallest = index;
                                count_of_smallest = enemy_spawns_count[index];
                            }
                        }

                        if(index_of_smallest < enemy_spawns.length) {
                            enemy_spawns[index_of_smallest] = [x, y];
                            enemy_spawns_count[index_of_smallest] = enemy_count;
                        }
                    }
                }
            }
        }
    }

    if(enemy_spawns.length > 0) {
        var spawn_index = 0;

        // Sort enemy spawns by number of enemies.
        for(selected = 0; selected < enemy_spawns.length - 1; ++selected) {
            for(focus = selected + 1; focus < enemy_spawns.length; ++focus) {
                if(enemy_spawns_count[focus] > enemy_spawns_count[selected]) {
                    var temp = enemy_spawns[selected];
                    enemy_spawns[selected] = enemy_spawns[focus];
                    enemy_spawns[focus] = temp;

                    temp = enemy_spawns_count[selected];
                    enemy_spawns_count[selected] = enemy_spawns_count[focus];
                    enemy_spawns_count[focus] = temp;
                }
            }
        }

        map[enemy_spawns[spawn_index][0]][enemy_spawns[spawn_index][1]].type = 4;
    } else if(safe_spawns.length > 0) {
        var spawn_index = safe_spawns.length*Math.random();

        map[safe_spawns[spawn_index][0]][safe_spawns[spawn_index][1]].type = 4;
    }

    return map;
}

function spawnGoldAndMaterialDrops(map, profitability, materialability)
{
    var path = [];
    var source;
    var waypoint_radius;
    var waypoint_thickness = 3;
    var waypoint_count = 2;
    var start_location;
    var end_location;
    var waypoints = [];
    var possible_waypoints = [[], []];
    var special_locations = [];
    var drop_spawn_locations = [];

    for(x = 1; x < map.length - 1; ++x) {
        for(y = 1; y < map[0].length - 1; ++y) {
           	switch(map[x][y].type) {
                case 1:
                    start_location = [x, y];
                    break;

                case 2:
                    end_location = [x, y];
                    break;

                case 4:
                    special_locations.push([x, y]);
                    break;
            }

            if(typeof start_location !== "undefined" && typeof end_location !== "undefined") {
                break;
            }
        }
    }

    // Find some waypoints to make the path interesting.
    waypoint_radius = 0.4*Math.sqrt(Math.pow(end_location[0] - start_location[0], 2) + Math.pow(end_location[1] - start_location[1], 2));

  	for(expand = 0; expand < waypoint_thickness; ++expand) {
        for(angle = 0; angle < 360; ++angle) {
         	var new_start = [Math.floor(0.5 + start_location[0] + (waypoint_radius + expand)*Math.cos(0.01745*angle)), Math.floor(0.5 + start_location[1] + (waypoint_radius + expand)*Math.sin(0.01745*angle))];
            var new_end = [Math.floor(0.5 + end_location[0] + (waypoint_radius + expand)*Math.cos(0.01745*angle)), Math.floor(0.5 + end_location[1] + (waypoint_radius + expand)*Math.sin(0.01745*angle))];

            if(new_start[0] > 1 && new_start[0] < map.length - 1 && new_start[1] > 1 && new_start[1] < map[0].length - 1 && !map[new_start[0]][new_start[1]].isSolid && map[new_start[0]][new_start[1]].type == 0) {
                possible_waypoints[0].push([new_start[0], new_start[1]]);
            }

            if(new_end[0] > 1 && new_end[0] < map.length - 1 && new_end[1] > 1 && new_end[1] < map[0].length - 1 && !map[new_end[0]][new_end[1]].isSolid && map[new_end[0]][new_end[1]].type == 0) {
                possible_waypoints[1].push([new_end[0], new_end[1]]);
            }
        }
    }

    // Cycle each set and path to each point, picking spawn areas in proper locations.
    var temp_waypoint = Math.floor(possible_waypoints[0].length*Math.random());
    waypoints.push([possible_waypoints[0][temp_waypoint][0], possible_waypoints[0][temp_waypoint][1]]);
    temp_waypoint = Math.floor(possible_waypoints[1].length*Math.random());
    waypoints.push([possible_waypoints[1][temp_waypoint][0], possible_waypoints[1][temp_waypoint][1]]);
    waypoints.push(end_location);

    source = [start_location[0], start_location[1]];

    // Build path using sequential breadth-first searches.
    while(waypoints.length > 0) {
        var local_path = [];
        var destination = waypoints.shift();
        var runner = [destination[0], destination[1]];
        var cell_queue = [[source[0], source[1]]];
        var history = new Array(map.length);

        for(index = 0; index < history.length; ++index) { history[index] = new Array(map[0].length); }
        history[source[0]][source[1]] = [-1, -1];

        while(cell_queue.length > 0) {
            var current = cell_queue.shift();
            var neighbors = [[current[0] + 1, current[1]], [current[0] - 1, current[1]], [current[0], current[1] + 1], [current[0], current[1] - 1]];

            if(current[0] == destination[0] && current[1] == destination[1]) {
                break;
            }

            for(index = 0; index < neighbors.length; ++index) {
                if(!map[neighbors[index][0]][neighbors[index][1]].isSolid && typeof history[neighbors[index][0]][neighbors[index][1]] === "undefined") {
                    history[neighbors[index][0]][neighbors[index][1]] = [current[0], current[1]];
                    cell_queue.push([neighbors[index][0], neighbors[index][1]]);
                }
            }
        }

        // Back track to splice into the full path.
        while(runner[0] != -1 && runner[1] != -1) {
            local_path.push([runner[0], runner[1]]);
            runner = history[runner[0]][runner[1]];
        }

        local_path.push([source[0], source[1]]);

        // Append to path.
        while(local_path.length > 0) {
            path.push(local_path.pop());
        }

        source = [destination[0], destination[1]];
    }

    // Print path for testing ...
    for(NARNIA = 0; NARNIA < path.length; ++NARNIA) {
        for(x = path[NARNIA][0] - 5; x <= path[NARNIA][0] + 5; ++x) {
            for(y = path[NARNIA][1] - 5; y <= path[NARNIA][1] + 5; ++y) {
                if(x > 1 && x < map.length - 1 && y > 1 && y < map[0].length - 1 && !map[x][y].isSolid) {
                    var neighbor_count = 0;

                    for(my = x - 1; my <= x + 1; ++my) {
                        for(poop = y - 1; poop <= y + 1; ++poop) {
                            if(my != x && poop != y && map[my][poop].isSolid) {
                                neighbor_count++;
                            }
                        }
                    }
                    //console.log(neighbor_count);
                    if(neighbor_count > 2 && map[x][y].type == 0) {
                        drop_spawn_locations.push([x, y]);
                    }
                }
            }
        }
    }

    // Drop some items in locations
    while(drop_spawn_locations.length > 0) {
        var location = drop_spawn_locations.pop();

        if(Math.random() <= materialability) {
            map[location[0]][location[1]].type = 6;
        } else if(Math.random() <= profitability) {
            map[location[0]][location[1]].type = 5;
        }
    }

    return map;
}

function spawnEnemies(map, population)
{
    var list_of_spawns = [];

    for(x = 1; x < map.length - 1; ++x) {
        for(y = 1; y < map[0].length - 1; ++y) {
            if(!map[x][y].isSolid && !map[x][y].isVisible && map[x][y].type == 0 && Math.random() <= 0.6*map[x][y].safety_normalized) {
                list_of_spawns.push([x, y]);
                map[x][y].type = 3;
            }
        }
    }

    while(list_of_spawns.length > population) {
        var spawn = list_of_spawns.pop();

        if(Math.random() > 0.3*map[spawn[0]][spawn[1]].safety_normalized) {
        	map[spawn[0]][spawn[1]].type = 0;
        } else {
            list_of_spawns.unshift(spawn);
        }
    }

    return map;
}

function applyLOS(map, location)
{
    for(angle = 0; angle < 720; ++angle) {
        var timeout = 0;
        var delta_x = Math.cos(0.01745*(0.5*angle));
        var delta_y = Math.sin(0.01745*(0.5*angle));
        var position_x = 0.5 + location[0];
        var position_y = 0.5 + location[1];

        while(!map[Math.floor(position_x)][Math.floor(position_y)].isSolid && ++timeout < 512) {
            position_x += delta_x;
            position_y += delta_y;

            map[Math.floor(position_x)][Math.floor(position_y)].isVisible = true;
        }
    }

    return map;
}

function pickExit(map)
{
    var region_left = 0;
    var region_top = 0;
    var region_right = map.length/2;
    var region_bottom = map[0].length/2;
    var possible_spawns = [];

    while(possible_spawns.length < 20) {
        var new_spawns = [];
        new_spawns.push([Math.floor(Math.random()*region_right), Math.floor(Math.random()*region_bottom)]);
        new_spawns.push([Math.floor(region_right + Math.random()*region_right), Math.floor(Math.random()*region_bottom)]);
        new_spawns.push([Math.floor(Math.random()*region_right), Math.floor(region_bottom + Math.random()*region_bottom)]);
        new_spawns.push([Math.floor(region_right + Math.random()*region_right), Math.floor(region_bottom + Math.random()*region_bottom)]);

        while(new_spawns.length > 0) {
            var duplicate = false;
            var spawn = new_spawns.pop();

            if(map[spawn[0]][spawn[1]].isSolid || map[spawn[0]][spawn[1]].type != 0 || map[spawn[0]][spawn[1]].safety === -1 || map[spawn[0]][spawn[1]].safety_normalized < 0.08) {
                continue;
            }

            for(index = 0; index < possible_spawns.length; ++index) {
                if(spawn[0] == possible_spawns[index][0] && spawn[1] == possible_spawns[index][1]) {
                    duplicate = true;
                    break;
                }
            }

            if(!duplicate) {
                possible_spawns.push([spawn[0], spawn[1]]);
            }
        }
    }

    // Sort list based on cell's safety in descending order.
    for(selected = 0; selected < possible_spawns.length - 1; ++selected) {
        for(focus = selected + 1; focus < possible_spawns.length; ++focus) {
            if(map[possible_spawns[focus][0]][possible_spawns[focus][1]].safety > map[possible_spawns[selected][0]][possible_spawns[selected][1]].safety) {
            	var temp = possible_spawns[selected];
                possible_spawns[selected] = possible_spawns[focus];
                possible_spawns[focus] = temp;
            }
        }
    }

    return possible_spawns[Math.floor(possible_spawns.length*Math.random()*Math.random()*Math.random())];
}

function initializeMap(width, height)
{
    var new_map = new Array(width);

    for(x = 0; x < width; ++x) {
        new_map[x] = new Array(height);

        for(y = 0; y < height; ++y) {
            new_map[x][y] = new Cell(true);
        }
    }

    return new_map;
}

function Cell(is_solid)
{
    this.isSolid = is_solid;
    this.type = 0;
    this.safety = -1;
    this.safety_normalized = -1.0;
    this.isVisible = false;

    this.clean = function() {
        this.isSolid = true;
        this.type = 0;
        this.safety = -1;
        this.safety_normalized = -1.0;
        this.isVisible = false;
    }

    this.clone = function(target) {
        this.isSolid = target.isSolid;
        this.type = target.type;
        this.safety = target.safety;
        this.safety_normalized = target.safety_normalized;
        this.isVisible = target.isVisible;
    }
}

function Miner(start_x, start_y, map)
{
    this.isAlive = true;
    this.isBored = false;
    this.x = start_x;
    this.y = start_y;
    this._world = map;

    this.mine = function() {
        if(this.isAlive) {
            var direction = Math.floor(4*Math.random());
            var valid_directions = this._get_valid_directions(true);
            var new_position = [-1, -1];

            if(valid_directions.length > 0) {
                new_position = this._translate_in_direction([this.x, this.y], valid_directions[direction%valid_directions.length]);

                // Clear space and move in direction
                this._world[new_position[0]][new_position[1]].isSolid = false;
                this.x = new_position[0];
                this.y = new_position[1];

                this.isBored = false;

                return true;
            } else {
                return false;
            }
        }
    }

    this._valid_space = function(x, y) { return x >= 1 && x < map.length - 1 && y >= 1 && y < map[0].length - 1; }

    this._get_valid_directions = function(test_if_minable) {
        var valid_directions = [];

        for(possible_direction = 0; possible_direction < 4; ++possible_direction) {
            var position = this._translate_in_direction([this.x, this.y], possible_direction);

            // Check valid direction.
            if(this._valid_space(position[0], position[1]) && (!test_if_minable || this._world[position[0]][position[1]].isSolid)) {
                valid_directions.push(possible_direction);
            }
        }

        return valid_directions;
    }

    this._translate_in_direction = function(position, direction) {
        switch(direction) {
			case 0: position[1] -= 1; break;
			case 1: position[0] += 1; break;
			case 2: position[1] += 1; break;
			case 3: position[0] -= 1; break;
    	}

        return position;
    }
}

function digCaverns(map, start_position, miner_spawn_rate, max_used_miners)
{
    var override = false;
    var miner_count = 1;
    var start_x = start_position[0];
    var start_y = start_position[1];

    var miners = [new Miner(start_x, start_y, map)];
    map[start_x][start_y].isSolid = false;
    map[start_x][start_y].type = 1;

    while(miners.length > 0 && miner_count < max_used_miners) {
        var miners_dead_this_frame = 0;

        for(miner_index = 0; miner_index < miners.length; ++miner_index) {
            if(miners[miner_index].mine()) {
                override = false; // If a successful mine has occured, disable the override.

                if(Math.random() <= miner_spawn_rate) {
                    var valid_directions = miners[miner_index]._get_valid_directions(true);
                    var new_miner_position = miners[miner_index]._translate_in_direction([miners[miner_index].x, miners[miner_index].y], valid_directions[Math.floor(valid_directions.length*Math.random())]);

                    map[new_miner_position[0]][new_miner_position[1]].isSolid = false;
                    miners.push(new Miner(new_miner_position[0], new_miner_position[1], map));
                    ++miner_count;
                }
            } else {
                if(miners.length - miners_dead_this_frame > 1) {
                    ++miners_dead_this_frame;
                    miners[miner_index].isAlive = false;
                } else {
                    if(override) { // If we've just run with no successful mine, terminate loop.
                        console.log("LOOP DETECTED. FORCEFULLY TERMINATING");
                        miner_count = max_used_miners;
                    } else {
                        override = true;
                    }

                    var possible_dig_sites = [];
                    var original_position = [miners[miner_index].x, miners[miner_index].y];

                    for(dig_x = 1; dig_x < map.length - 1; ++dig_x) {
                        for(dig_y = 1; dig_y < map[0].length - 1; ++dig_y) {
                            miners[miner_index].x = dig_x;
                            miners[miner_index].y = dig_y;
                            if(!map[dig_x][dig_y].isSolid && miners[miner_index]._get_valid_directions(true).length > 0) {
                                possible_dig_sites.push([dig_x, dig_y]);
                            }
                        }
                    }

                    miners[miner_index].x = original_position[0];
                    miners[miner_index].y = original_position[1];

                    if(possible_dig_sites.length > 0) {
                        var dig_site = possible_dig_sites[Math.floor(possible_dig_sites.length*Math.random())];
                        miners[miner_index].x = dig_site[0];
                        miners[miner_index].y = dig_site[1];
                    }
                }
            }
        }

        // Update miner list.
        var live_miners = [];

        for(miner_index = 0; miner_index < miners.length; ++miner_index) {
            if(miners[miner_index].isAlive) {
                live_miners.push(miners[miner_index]);
            } else {
                delete miners[miner_index];
            }
        }

        miners = live_miners;
    }

    return map;
}

function smoothMap(map, passes)
{
    var old_map = new Array(map.length);

    for(x = 0; x < map.length; ++x) {
        old_map[x] = new Array(map[0].length);
    }

    for(pass = 0; pass < passes; ++pass) {
        for(x = 0; x < map.length; ++x) {
            for(y = 0; y < map[0].length; ++y) {
                old_map[x][y] = new Cell(true);
                old_map[x][y].clone(map[x][y]);
            }
        }

        for(x = 0; x < map.length; ++x) {
            for(y = 0; y < map[0].length; ++y) {
                var neighbors = 0;

                for(adj_x = x - 1; adj_x <= x + 1; ++adj_x) {
                    for(adj_y = y - 1; adj_y <= y + 1; ++adj_y) {
                        if(adj_x == x && adj_y == y) {
                            continue;
                        }

                        if(adj_x < 0 || adj_x >= map.length || adj_y < 0 || adj_y >=  map[0].length || old_map[adj_x][adj_y].isSolid) {
                            neighbors++;
                        }
                    }
                }

                if(old_map[x][y].isSolid) {
                    if(neighbors <= 2) {
                        map[x][y].isSolid = false;
                    }
                }
            }
        }
    }

    return map;
}

function applySafetyZone(map, position, radius, max_safety) {
    var cell_queue = [position];
    var current_safety_value = 0;
    var new_queue = [];

    var frame = 0;
    var frame_limit = 1500;

    var visited = new Array(map.length);

    for(x = 0; x < visited.length; ++x) {
        visited[x] = new Array(map[0].length);

        for(y = 0; y < visited[0].length; ++y) {
            visited[x][y] = false;
        }
    }

    while(cell_queue.length > 0 && ++frame < frame_limit) {
        for(cell_index = 0; cell_index < cell_queue.length; ++cell_index) {
            var current_cell = map[cell_queue[cell_index][0]][cell_queue[cell_index][1]];
            current_cell.safety = (current_cell.safety > current_safety_value || current_cell.safety == -1 ? current_safety_value : current_cell.safety);
            visited[cell_queue[cell_index][0]][cell_queue[cell_index][1]] = true;

            for(direction = 0; direction < 4; ++direction) {
                var adj_position = [cell_queue[cell_index][0], cell_queue[cell_index][1]];

                switch(direction) {
                    case 0: adj_position[1] -= 1; break;
                    case 1: adj_position[0] += 1; break;
                    case 2: adj_position[1] += 1; break;
                    case 3: adj_position[0] -= 1; break;
                }

                if(adj_position[0] >= 1 && adj_position[0] < map.length - 1 && adj_position[1] >= 1 && adj_position[1] < map[0].length - 1 && !map[adj_position[0]][adj_position[1]].isSolid && !visited[adj_position[0]][adj_position[1]]) {
                    var duplicate = false;

                    for(index = 0; index < new_queue.length; ++index) {
                        if(new_queue[index][0] == adj_position[0] && new_queue [index][1] == adj_position[1]) {
                            duplicate = true;
                            break;
                        }
                    }

                    if(!duplicate) {
                        new_queue.push([adj_position[0], adj_position[1]]);
                    }
                }
            }
        }

        // Setup environment for next pass
        current_safety_value++;
        cell_queue = [];

        while(new_queue.length > 0) {
            cell_queue.push(new_queue.pop());
        }
    }

    // Apply radius modifier.
    for(x = 1; x < map.length - 1; ++x) {
        for(y = 1; y < map[0].length - 1; ++y) {
            if(map[x][y].safety >= 0) {
                map[x][y].safety -= radius;

                if(map[x][y].safety < 0) {
                    map[x][y].safety = 0;
                } else if(map[x][y].safety > max_safety) {
                    map[x][y].safety = max_safety;
                }

                map[x][y].safety_normalized = (1.0*map[x][y].safety)/max_safety;
            }
        }
    }

    if(frame === frame_limit) {
        console.log("Safety sequence timed out after " + frame + (frame != 1 ? "s" : "") + ".");
    }

    return map;
}


function drawMap(map, offset, zoom_factor, cell_size, draw_safety_map, draw_visibility) {
    context.fillStyle = "#000";
    context.fillRect(0, 0, 400, 550);

    for(x = 0; x < map.length; ++x) {
        for(y = 0; y < map[0].length; ++y) {
            if(map[x][y].isSolid) {
                context.fillStyle = "#000";
                continue;
            } else {
                switch(map[x][y].type) {
                    case 1:
                        context.fillStyle = "#12ae12";
                        break;

                    case 2:
                        context.fillStyle = "#db0b0b";
                        break;

                    case 3:
                        context.fillStyle = "#ffae00";
                        break;

                  	case 4:
                        context.fillStyle = "#ff00ff";
                        break;

                    case 5:
                        context.fillStyle = "#707002";
                        break;

                    case 6:
                        context.fillStyle = "#25dee8";
                        break;

                    case 665:
                        context.fillStyle = "#ffff00";
                        break;

                    case 666:
                        context.fillStyle = "#ff00ff";
                        break;

                    default:
                        if(draw_visibility && map[x][y].isVisible) {
                            context.fillStyle = "#ffff00";
                        } else if(draw_safety_map && map[x][y].safety >= 0) {
                            var red = Math.floor((map[x][y].safety_normalized)*255);
                            var green = 0;
                            var blue = Math.floor((1 - map[x][y].safety_normalized)*255);

                            var red_string = "00" + red.toString(16);
                            var green_string = "00" + green.toString(16);
                            var blue_string = "00" + blue.toString(16);

                            red_string = red_string.substring(red_string.length - 2);
                            green_string = green_string.substring(green_string.length - 2);
                            blue_string = blue_string.substring(blue_string.length - 2);

                            context.fillStyle = "#" + red_string + green_string + blue_string;
                        } else {
                        	context.fillStyle = "#fff";
                        }
                        break;
                }
            }

            context.fillRect(zoom_factor*(offset[0] + cell_size*x), zoom_factor*(offset[1] + cell_size*y), zoom_factor*cell_size, zoom_factor*cell_size);
        }
    }
}
