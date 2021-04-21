function AboutCtrl($scope, $http, $routeParams) {
    $scope.$emit('newPageLoaded', {
        title: 'IncTrak – About us',
        description: 'Welcome to IncTrak, we’re dedicated to giving employees access to their goals.'
    });
}