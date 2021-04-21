function PlansCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Plans',
        description: 'Create your COmpany Stock option Plans.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.showPlans = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.planKey = null;
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.optioneePlanGet(" - Loading...", "/api/company/plans/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadPlan = function (key) {
        var uuid = $cookies.UUID;
        $scope.planKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Plans - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/plan/" + $scope.planKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs =[{url : "#/company_plans/" + Math.random() + "/", name: "> Plans"}];
                        $scope.listItems =[];
                        $scope.showPlans = false;
                        $scope.showPlan = true;
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
            $scope.save_message = "Please register or login to set Plans - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var plan = $scope.data.Plan;
            var planError = "";
            if (!plan.NAME)
                planError += "Plan Name ";
            if (plan.TOTAL_SHARES < 1)
                planError += "Round ";
            if ( planError )
                error += "Plan has invalid (" + planError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/plan/", { Key: $scope.planKey, UUID: uuid, Data: plan }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadPlan(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your Plans, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };

    $scope.removePlan = function (key) {
        var uuid = $cookies.UUID;
        $scope.planKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Plans - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/plan/" +$scope.planKey + "/" +uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Plans;
                        $scope.breadCrumbs =[];
                        $scope.showPlans = true;
                        $scope.showPlan = false;
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

