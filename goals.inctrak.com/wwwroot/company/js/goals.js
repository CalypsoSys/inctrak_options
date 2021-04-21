function GoalsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Goals',
        description: 'Create your Company goals.',
        keywords: 'sprint development goal startup employee'
    });

    $scope.apiSearchUrl = '/api/company/goals/';
    $scope.apiItemUrl = '/api/company/goal/';
    SearchLoadCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.showGoals = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.goalKey = null;

    $scope.member = {};
    $scope.member.selected = undefined;

    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.searchChange(true);

    $scope.refreshMembers = function (searchString) {
        if (!searchString) {
            searchString = "_____";
        }
        $scope.baseGoalGet(" - Loading...", "/api/company/members/" + searchString + "/InLine/").baseGoalComplete(function (data) {
            $scope.members = data;
        });
    }

    $scope.loadGoal = function (key) {
        var uuid = $cookies.UUID;
        $scope.goalKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set goal - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalGet(" - Loading...", "/api/company/goal/" + $scope.goalKey + "/" + uuid + "/").
                baseGoalComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_goals/" + Math.random() + "/", name: "> Goals" }];
                        $scope.listItems = [];
                        $scope.showGoals = false;
                        $scope.showGoal = true;
                        $scope.data = data;
                        $scope.GroupKeyCheck = data.GoalSet.GroupKeyCheck;
                        if (data.Member)
                            $scope.member.selected = data.Member;
                        else
                            $scope.refreshMembers();
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    $scope.delGoal = function (goal) {
        var index = $scope.data.Goals.indexOf(goal);
        $scope.data.Goals.splice(index, 1);
    };

    $scope.addGoal = function () {
        $scope.data.Goals.push({ GroupKeyCheck: $scope.GroupKeyCheck })
    };

    // process the form
    $scope.processForm = function () {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set goal - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var goalSet = $scope.data.GoalSet;
            var goalError = "";
            if ($scope.member.selected)
                goalSet.MEMBER_FK = $scope.member.selected.MEMBER_PK;

            if (!goalSet.MEMBER_FK)
                goalError += " Member ";
            if (!goalSet.TEAM_FK)
                goalError += " Team ";
            if (!goalSet.SCHEDULE_FK)
                goalError += " Schedule ";

            var goals = [];
            var goalsError = $scope.validateGoals(goals);
            if (goalsError)
                goalError += goalsError;

            if (goalError)
                error += "Goal has invalid (" + goalError + ")\r\n";

            if (error) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/goal/", { Key: $scope.goalKey, UUID: uuid, Data: goalSet, Children: goals }).
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

    $scope.validateGoals = function (goals) {
        var error = "";

        var descriptions = [];
        for (var i = 0; i < $scope.data.Goals.length; i++) {
            var goal = $scope.data.Goals[i];

            var goalError = "";
            if (!goal.DESCRIPTION)
                goalError += "Description ";
            else if (descriptions.includes(goal.DESCRIPTION))
                goalError += "Duplicate description [" + goal.DESCRIPTION + "]";
            else
                descriptions.push(goal.DESCRIPTION);
            if (!goal.IMPORTANCE_TYPE_FK)
                goalError += "Importance Type ";
            goals.push(goal);
            if (goalError)
                error += "Goal " + (i + 1) + " has invalid (" + goalError + ")\r\n";
        }

        return error;
    }


    $scope.removeGoals = function (key) {
        var uuid = $cookies.UUID;
        $scope.goalKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove goal - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalDelete(" - Loading...", "/api/company/goal/" + $scope.goalKey + "/" + uuid + "/").
                baseGoalRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Goals;
                        $scope.breadCrumbs = [];
                        $scope.showGoals = true;
                        $scope.showGoal = false;
                        $scope.data = null;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

}

