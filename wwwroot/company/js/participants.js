function ParticipantsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Participant',
        description: 'Create your Company Stock option Vesting participant.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.apiSearchUrl = '/api/company/participants/';
    $scope.apiItemUrl = '/api/company/participant/';
    SearchLoadOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.showParticipants = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.participantKey = null;
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.searchChange(true);

    /*
    $scope.optioneePlanGet(" - Loading...", "/api/company/participants/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });
    */

    $scope.loadParticipant = function (key) {
        var uuid = $cookies.UUID;
        $scope.participantKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set participant - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/participant/" + $scope.participantKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_participants/" + Math.random() + "/", name: "> Participants" }];
                        $scope.listItems = [];
                        $scope.showParticipants = false;
                        $scope.showParticipant = true;
                        $scope.data = data;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    $scope.isNewUser = function (participant) {
        return !participant || participant.USER_FK == null;
    };

    $scope.disableUnEmail = function (participant) {
        return !participant || participant.USER_ACTION == "none" || participant.USER_ACTION == "delete_user";
    }

    // process the form
    $scope.processForm = function () {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set participant - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var participant = $scope.data.Participant;
            var participantError = "";
            if (!participant.NAME)
                participantError += "Participant Name ";
            if (participant.USER_ACTION == "create_user" || participant.USER_ACTION == "update_user") {
                if (!participant.GOOGLE_USER && !participant.USER_NAME)
                    participantError += "Participant username ";
                if (!participant.EMAIL_ADDRESS)
                    participantError += "Participant email ";
            }

            if (participantError)
                error += "Participant has invalid (" + participantError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/participant/", { Key: $scope.participantKey, UUID: uuid, Data: participant }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadParticipant(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your participant, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };


    $scope.removeParticipant = function (key) {
        var uuid = $cookies.UUID;
        $scope.participantKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove participant - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/participant/" + $scope.participantKey + "/" + uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Participants;
                        $scope.breadCrumbs = [];
                        $scope.showParticipants = true;
                        $scope.showParticipant = false;
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

