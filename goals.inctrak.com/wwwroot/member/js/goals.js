function MemberGoalsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Goals',
        description: 'Create your Company goal.',
        keywords: 'sprint development goal startup employee'
    });

    $scope.showGoals = true;
    $scope.showDetails = false;
    $scope.showGoalHistory = false;
    $scope.listItems = [];
    $scope.goalKey = null;
    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.baseGoalGet(" - Loading...", "/api/member/goals/").baseGoalComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadGoal = function (key) {
        var uuid = $cookies.UUID;
        $scope.goalKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to get goals - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalGet(" - Loading...", "/api/member/goal/" + $scope.goalKey + "/" + uuid + "/").
                baseGoalComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/member_goals/" + Math.random() + "/", name: "> Goal" }];
                        $scope.listItems = [];
                        $scope.showGoals = false;
                        $scope.showGoal = true;
                        $scope.data = data;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    $scope.showHistory = function () {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to get goals - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalGet(" - Loading...", "/api/member/goal_history/" + $scope.goalKey + "/" + uuid + "/").
                baseGoalComplete(function (data) {
                    if (data.success) {
                        $scope.showGoalHistory = true;
                        $scope.goalHistory = data.GoalHistory;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    $scope.updateMemberComment = function (goalPk, memberComment) {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set goal - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var goalError = "";

            if (!goalPk)
                goalError += " Goal ";
            if (!memberComment)
                goalError += " Comment ";

            if (goalError)
                error += "Goal has invalid (" + goalError + ")\r\n";

            if (error) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/member/goal/", { UUID: uuid, Key: goalPk, Data: memberComment }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadGoal(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your goal, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };
}

