function TerminationsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Termination Dates',
        description: 'Create your Company Stock option Vesting termination dates.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.showTerminations = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.terminationKey = null;
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.optioneePlanGet(" - Loading...", "/api/company/terminations/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadTermination = function (key) {
        var uuid = $cookies.UUID;
        $scope.terminationKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set termination dates - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/termination/" + $scope.terminationKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_terminations/" + Math.random() + "/", name: "> Termination Dates" }];
                        $scope.listItems = [];
                        $scope.showTerminations = false;
                        $scope.showTermination = true;
                        if (data && data.Termination && data.Termination.ABSOLUTE_DATE) {
                            data.Termination.ABSOLUTE_DATE = data.Termination.ABSOLUTE_DATE.split('T')[0];
                        }
                        if (data && data.Termination && data.Termination.SPECIFIC_DATE) {
                            data.Termination.SPECIFIC_DATE = data.Termination.SPECIFIC_DATE.split('T')[0];
                        }
                        $scope.data = data;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    $scope.isSpecificDate = function () {
        var _termSpecificDate = 3;

        var isSpecific = false;
        if ($scope.data && !$scope.data.Termination.IS_ABSOLUTE) {
            if ($scope.data.Termination.TERM_FROM_FK) {
                $scope.data.TermFromType.forEach(function (item, key) {
                    if ($scope.data.Termination.TERM_FROM_FK == item.TERM_FROM_PK && item.ID == _termSpecificDate)
                        isSpecific = true;
                });
            }
        }

        return isSpecific;
    }

    // process the form
    $scope.processForm = function () {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set termination dates - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var termination = $scope.data.Termination;
            var terminationError = "";
            if (!termination.NAME)
                terminationError +=  "Termination Name ";
            if (termination.IS_ABSOLUTE) {
                if (!termination.ABSOLUTE_DATE)
                    terminationError += " absolute date ";
            } else {
                if (!termination.TERM_FROM_FK)
                    terminationError += " from date ";
                else if ($scope.isSpecificDate() && !termination.SPECIFIC_DATE) {
                    terminationError += " specific date ";
                }

                if (!termination.YEARS && !termination.MONTHS && !termination.DAYS) {
                    terminationError += " years/months/days ";
                }
            }

            if (terminationError)
                error += "Termination date has invalid (" + terminationError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/termination/", { Key: $scope.terminationKey, UUID: uuid, Data: termination }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadTermination(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your termination date, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };


    $scope.removeTermination = function (key) {
        var uuid = $cookies.UUID;
        $scope.terminationKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove termination dates - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/termination/" + $scope.terminationKey + "/" + uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Terminations;
                        $scope.breadCrumbs = [];
                        $scope.showTerminations = true;
                        $scope.showTermination = false;
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

