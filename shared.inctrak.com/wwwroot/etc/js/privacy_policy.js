function PrivacyCtrl($scope, $http, $routeParams) {
    $scope.$emit('newPageLoaded', {
        title: 'Terms and Conditions',
        description: 'Read here our terms and conditions for more information. Go through the full terms and conditions with a quick summary of some important points to help and guide you.',
    });
}