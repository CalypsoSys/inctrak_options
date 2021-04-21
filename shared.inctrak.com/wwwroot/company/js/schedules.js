function SchedulesCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Schedules',
        description: 'Create your Company Stock option Vesting Schedules.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.showSchedules = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.scheduleKey = null;
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    PeriodCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.optioneePlanGet(" - Loading...", "/api/company/schedules/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadSchedule = function (key) {
        var uuid = $cookies.UUID;
        $scope.scheduleKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set schedules - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
            $scope.GroupKeyCheck = key;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/schedule/" + $scope.scheduleKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{url : "#/company_schedules/" + Math.random() + "/", name: "> Schedules"}];
                        $scope.listItems = [];
                        $scope.showSchedules = false;
                        $scope.showSchedule = true;
                        $scope.data = data;
                        $scope.GroupKeyCheck = data.Schedule.GroupKeyCheck;
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
            $scope.save_message = "Please register or login to set schedules - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var schedule = $scope.data.Schedule;
            var scheduleError = "";
            if (!schedule.NAME)
                scheduleError += "Schedule Name ";

            var periods = [];
            var periodsError = $scope.validatePeriods(periods);
            if (periodsError)
                scheduleError += periodsError;

            if (scheduleError)
                error += "Schedule has invalid (" + scheduleError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/schedule/", { Key: $scope.scheduleKey, UUID: uuid, Data: schedule, Children: periods }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadSchedule(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your schedule, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };


    $scope.removeSchedule = function (key) {
        var uuid = $cookies.UUID;
        $scope.scheduleKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove schedules - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/schedule/" +$scope.scheduleKey + "/" +uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Schedules;
                        $scope.breadCrumbs = [];
                        $scope.showSchedules = true;
                        $scope.showSchedule = false;
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

