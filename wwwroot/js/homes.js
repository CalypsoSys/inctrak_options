function HomesCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'IncTrak',
        description: 'IncTrak provides easy access for employers to allow participants to manager their stock qand options.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.dialogTemplate = '/common/dialog.html';
    $scope.periodTemplate = '/common/periods.html';
    $scope.vestScheduleTemplate = '/common/vestschedule.html';
    $scope.isShowPeriod = true;
    $scope.isShowDetails = true;
    $scope.isShowTotal = false;
    $scope.isShowVested = false;
    $scope.showVestSchedule = false;

    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    if ($scope.isAdmin()) {
        $scope.optioneePlanGet(" - Loading...", "/api/company/summary/").optioneePlanComplete(function (data) {
            $scope.breadCrumbs = [];
            $scope.data = data;
            $scope.adminCounts(data.Counts);
        });
    } else if ($scope.isOptionee()) {
        $scope.optioneePlanGet(" - Loading...", "/api/optionee/summary/").optioneePlanComplete(function (data) {
            $scope.breadCrumbs = [];
            $scope.data = data;
            $scope.optioneeOverTime(data.OverTime);
        });
    } else if (!$scope.loggedIn()) {
        PeriodCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
        $scope.optioneePlanGet(" - Loading...", "/api/optionee/quick/").optioneePlanComplete(function (data) {
            $scope.breadCrumbs = [];
            $scope.data = data;
        });
    }

    $scope.adminCounts = function (data) {
        var custom_bubble_chart = (function (d3) {
            "use strict";

            var width = 940,
                height = 600,
                layout_gravity = -0.01,
                damper = 0.01,
                nodes = [],
                vis, force, parentNodes, radius_scale, circleNodes, textNodes;

            var center = { x: (width-200) / 2, y: height / 2 };

            var fill_color = d3.scale.category20c();

            function custom_chart(data) {
                var max_amount = d3.max(data, function (d) { return parseInt(d.count, 10); });
                radius_scale = d3.scale.pow().exponent(0.5).domain([0, max_amount]).range([2, 85]);

                //create node objects from original data
                //that will serve as the data behind each
                //bubble in the vis, then add each node
                //to nodes to be used later
                data.forEach(function (d) {
                    var node = {
                        id: d.name,
                        radius: radius_scale(parseInt(d.count, 10)),
                        value: d.count,
                        name: d.name,
                        order: d.order,
                        x: Math.random() * 900,
                        y: Math.random() * 800
                    };
                    nodes.push(node);
                });

                nodes.sort(function (a, b) { return a.order - b.order; });

                vis = d3.select("#company_counts").append("svg")
                            //.attr("width", width)
                            //.attr("height", height)
                            .attr("viewBox", "0 -20 940 600")
                            .attr("id", "svg_vis");

                parentNodes = vis.selectAll("circle")
                             .data(nodes, function (d) { return d.name; });

                var g = parentNodes.enter().append("g");

                textNodes = g.append("text")
                  .attr("dx", function (d) { return -25 })
                  .text(function (d) { return d.name; })

                circleNodes = g.append("circle")
                  .attr("r", 0)
                  .attr("fill", function (d) { return fill_color(d.name); })
                  .attr("opacity", .6)
                  .attr("stroke-width", 2)
                  .attr("stroke", function (d) { return d3.rgb(fill_color(d.name)).darker(); })
                  .attr("id", function (d) { return "bubble_" + d.name; });

                g.append("rect")
                    .attr("x", width - 350)
                    .attr("y", function (d, i) { return (i * 20) - 7; })
                    .attr("width", 10)
                    .attr("height", 10)
                    .style("fill", function (d) { return fill_color(d.name); });
                g.append("text")
                    .attr("x", width - 335)
                    .attr("y", function (d, i) { return (i * 20); })
                    .text(function (d) { return " " + d.name + " " + d.value.toLocaleString(); })

                circleNodes.transition().duration(2000).attr("r", function (d) { return d.radius; });

            }

            function charge(d) {
                return -Math.pow(d.radius, 2.0) / 8;
            }

            function start() {
                force = d3.layout.force()
                        .nodes(nodes)
                        .size([width, height]);
            }

            function display_group_all() {
                force.gravity(layout_gravity)
                     .charge(charge)
                     .friction(0.9)
                     .on("tick", function (e) {
                         parentNodes.each(move_towards_center(e.alpha))
                                .attr("x", function (d) { return d.x; })
                                .attr("y", function (d) { return d.y; });
                         circleNodes.each(move_towards_center(e.alpha))
                                .attr("cx", function (d) { return d.x; })
                                .attr("cy", function (d) { return d.y; });
                         textNodes.each(move_towards_center(e.alpha))
                                .attr("x", function (d) { return d.x; })
                                .attr("y", function (d) { return d.y; });
                     });
                force.start();
            }

            function move_towards_center(alpha) {
                return function (d) {
                    d.x = d.x + (center.x - d.x) * (damper + 0.02) * alpha;
                    d.y = d.y + (center.y - d.y) * (damper + 0.02) * alpha;
                };
            }

            var my_mod = {};
            my_mod.init = function (_data) {
                custom_chart(_data);
                start();
            };

            my_mod.display_all = display_group_all;
            my_mod.view = function () {
                display_group_all();
            };

            return my_mod;
        })(d3);

        custom_bubble_chart.init(data);
        custom_bubble_chart.view();
    }

    $scope.optioneeOverTime = function (data) {
        var groups = new vis.DataSet();
        var dataset = new vis.DataSet();
        var minDate, maxDate;
        var groupId = 0;
        jQuery.each(data, function (key, val) {
            if (key == "MinDate")
                minDate = val;
            else if (key == "MaxDate")
                maxDate = val;
            else {

                groups.add({
                    id: groupId,
                    content: key,
                    options: { drawPoints: false }
                });
                var list = [];
                jQuery.each(val, function (date, count) {
                    list.push({ x: date, y: count, group: groupId });
                });
                dataset.add(list);
                ++groupId;
            }
        });
        var options = {
            //legend: true,
            legend:{left:{position:"top-right"}},
            start: minDate,
            end: maxDate
        };
        var container = document.getElementById('grants_over_time');
        var graph2d = new vis.Graph2d(container, dataset, groups, options);
    }

    $scope.processQuickForm = function () {
        var error = "";
        var grant = $scope.data.Grant;
        var quickError = "";
        if (!grant.VESTING_START)
            quickError += " Vesting Start Date ";
        if (!grant.SHARES)
            quickError += " Shares ";

        var periods = [];
        var periodsError = $scope.validatePeriods(periods);
        if (periodsError)
            quickError += periodsError;
        if (quickError)
            error += "Quick schedule has invalid (" + quickError + ")\r\n";

        if (error) {
            $scope.save_message = error;
            $scope.showDialog = true;
        } else {
            $scope.preBusy();
            $http.post("/api/optionee/quick/", { Data: grant, Children: periods }).
                success(function (data) {
                    $scope.save_message = data.message;
                    $scope.postBusy();
                    $scope.data = data;
                    if (data && data.Grant && data.Grant.VESTING_START) {
                        data.Grant.VESTING_START = data.Grant.VESTING_START.split('T')[0];
                    }

                    $scope.showVestSchedule = true;
                }).error(function (data) {
                    $scope.save_message = "An error occured creating your schedule, try again?";
                    $scope.showDialog = true;
                    $scope.postBusy();
                });
        }
    }
}