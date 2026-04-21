function GrantsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Grant',
        description: 'Create your Company Stock option grant.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.apiSearchUrl = '/api/company/grants/';
    $scope.apiItemUrl = '/api/company/grant/';
    SearchLoadOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.showGrants = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.grantKey = null;

    $scope.participant = {};
    $scope.participant.selected = undefined;

    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.searchChange(true);

    $scope.refreshParticipants = function (searchString) {
        if (!searchString) {
            searchString = "_____";
        }
        $scope.optioneePlanGet(" - Loading...", "/api/company/participants/" + searchString + "/InLine/").optioneePlanComplete(function (data) {
            $scope.participants = data;
        });
    }

    $scope.loadGrant = function (key) {
        var uuid = $cookies.UUID;
        $scope.grantKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set grant - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/grant/" + $scope.grantKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_grants/" + Math.random() + "/", name: "> Grants" }];
                        $scope.listItems = [];
                        $scope.showGrants = false;
                        $scope.showGrant = true;
                        $scope.data = data;
                        if (data && data.Grant && data.Grant.DATE_OF_GRANT) {
                            data.Grant.DATE_OF_GRANT = data.Grant.DATE_OF_GRANT.split('T')[0];
                        }
                        if (data && data.Grant && data.Grant.VESTING_START) {
                            data.Grant.VESTING_START = data.Grant.VESTING_START.split('T')[0];
                        }

                        if (data.Participant)
                            $scope.participant.selected = data.Participant;
                        else
                            $scope.refreshParticipants();
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
            $scope.save_message = "Please register or login to set grant - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var grant = $scope.data.Grant;
            var grantError = "";
            if ($scope.participant.selected)
                grant.PARTICIPANT_FK = $scope.participant.selected.PARTICIPANT_PK;

            if (!grant.PARTICIPANT_FK)
                grantError += " Participant ";
            if (!grant.PLAN_FK)
                grantError += " Plan ";
            if (!grant.VESTING_SCHEDULE_FK)
                grantError += " Vesting Schedule ";
            if (!grant.DATE_OF_GRANT)
                grantError += " Date of Grant ";
            if (!grant.VESTING_START)
                grantError += " Vesting Start Date ";
            if (!grant.OPTION_PRICE)
                grantError += " Option Price ";
            if (!grant.SHARES)
                grantError += " Shares ";
            if (!grant.TERMINATION_FK)
                grantError += " Termination ";

            if (grantError)
                error += "Grant has invalid (" + grantError + ")\r\n";

            if (error) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/grant/", { Key: $scope.grantKey, UUID: uuid, Data: grant }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadGrant(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your grant, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };


    $scope.removeGrant = function (key) {
        var uuid = $cookies.UUID;
        $scope.grantKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove grant - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/grant/" + $scope.grantKey + "/" + uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Grants;
                        $scope.breadCrumbs = [];
                        $scope.showGrants = true;
                        $scope.showGrant = false;
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

