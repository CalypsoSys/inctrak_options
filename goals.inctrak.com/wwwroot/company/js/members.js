function MembersCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Member',
        description: 'Create your Company Members.',
        keywords: 'sprint development goal startup employee'
    });

    $scope.apiSearchUrl = '/api/company/members/';
    $scope.apiItemUrl = '/api/company/member/';
    SearchLoadCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.showMembers = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.memberKey = null;
    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.searchChange(true);

    /*
    $scope.baseGoalGet(" - Loading...", "/api/company/members/").baseGoalComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });
    */

    $scope.loadMember = function (key) {
        var uuid = $cookies.UUID;
        $scope.memberKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set member - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalGet(" - Loading...", "/api/company/member/" + $scope.memberKey + "/" + uuid + "/").
                baseGoalComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_members/" + Math.random() + "/", name: "> Members" }];
                        $scope.listItems = [];
                        $scope.showMembers = false;
                        $scope.showMember = true;
                        $scope.data = data;
                    } else {
                        $scope.save_message = data.message;
                        $scope.navigateUrl = null;
                        $scope.showDialog = true;
                    }
                })
        }
    };

    $scope.isNewUser = function (member) {
        return !member || member.USER_FK == null;
    };

    $scope.disableUnEmail = function (member) {
        return !member || member.USER_ACTION == "none" || member.USER_ACTION == "delete_user";
    }

    // process the form
    $scope.processForm = function () {
        var uuid = $cookies.UUID;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set member - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var member = $scope.data.Member;
            var memberError = "";
            if (!member.NAME)
                memberError += "Member Name ";
            if (member.USER_ACTION == "create_user" || member.USER_ACTION == "update_user") {
                if (!member.GOOGLE_USER && !member.USER_NAME)
                    memberError += "Member username ";
                if (!member.EMAIL_ADDRESS)
                    memberError += "Member email ";
            }

            if (memberError)
                error += "Member has invalid (" + memberError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/member/", { Key: $scope.memberKey, UUID: uuid, Data: member }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadMember(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your member, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };


    $scope.removeMember = function (key) {
        var uuid = $cookies.UUID;
        $scope.memberKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove member - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalDelete(" - Loading...", "/api/company/member/" + $scope.memberKey + "/" + uuid + "/").
                baseGoalRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Members;
                        $scope.breadCrumbs = [];
                        $scope.showMembers = true;
                        $scope.showMember = false;
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

