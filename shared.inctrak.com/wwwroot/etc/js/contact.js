function ContactCtrl($scope, $http, $cookies, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Contact us',
        description: 'Want to tell us something? Got a business query? Write to us on the enquiry form or drop in at our offices and get answer for your every query.',
    });

    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.showDialog = false;
    $scope.save_message = "";
    $scope.messageTypes = [];
    $scope.optioneePlanGet(" - Loading...", "/api/feedback/message_types/").optioneePlanComplete(function (data) {
        $scope.messageTypes = data;
    });

    $scope.optioneePlanGet(" - Loading...", "/api/feedback/get_message/").optioneePlanComplete(function (data) {
        $scope.formData = data;
    });

    // process the form
    $scope.processForm = function () {
        if (!$scope.formData.Message && !$scope.formData.Subject) {
            $scope.save_message = "Please enter at least a subject or message - thx";
            $scope.showDialog = true;
            return;
        }
        if (!$scope.formData.EmailAddress && !$scope.formData.Name) {
            $scope.save_message = "Please enter at least a email address or name - thx";
            $scope.showDialog = true;
            return;
        }
        $scope.preBusy();
        $http.post("/api/feedback/save_message/", $scope.formData).
            success(function (data) {
                $scope.save_message = data.message;
                if (data.success) {
                    $scope.formData.EmailAddress = "";
                    $scope.formData.Name = "";
                    $scope.formData.Subject = "";
                    $scope.formData.Message = "";
                }
                $scope.showDialog = true;
                $scope.postBusy();
            }).error(function (data) {
                $scope.save_message = "An error occured submnitting corespondence";
                $scope.showDialog = true;
                $scope.postBusy();
            });
    };
}
