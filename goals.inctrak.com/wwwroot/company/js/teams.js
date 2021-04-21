function TeamsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Teams',
        description: 'Create your Company Teams.',
        keywords: 'sprint development goal startup employee'
    });

    $scope.showTeams = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.teamKey = null;
    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.baseGoalGet(" - Loading...", "/api/company/teams/").baseGoalComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadTeam = function (key) {
        var uuid = $cookies.UUID;
        $scope.teamKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Teams - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalGet(" - Loading...", "/api/company/team/" + $scope.teamKey + "/" + uuid + "/").
                baseGoalComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_teams/" + Math.random() + "/", name: "> Teams"}];
                        $scope.listItems =[];
                        $scope.showTeams = false;
                        $scope.showTeam = true;
                        $scope.data = data;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    // process the form
    $scope.processForm = function () {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Teams - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var team = $scope.data.Team;
            var teamError = "";
            if (!team.NAME)
                teamError += "Team Name ";
            if ( teamError )
                error += "Team has invalid (" + teamError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/team/", { Key: $scope.teamKey, UUID: uuid, Data: team }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadTeam(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your Teams, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };

    $scope.removeTeam = function (key) {
        var uuid = $cookies.UUID;
        $scope.teamKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Teams - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalDelete(" - Loading...", "/api/company/team/" +$scope.teamKey + "/" +uuid + "/").
                baseGoalRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Teams;
                        $scope.breadCrumbs =[];
                        $scope.showTeams = true;
                        $scope.showTeam = false;
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

