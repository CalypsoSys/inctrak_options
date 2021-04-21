function SearchLoadCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.searchTemplate = '/common/searchload.html';
    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);
    $scope.searchString = "";
    $scope.searchType = "all";
    $scope.showSearchResults = false;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.data = {};
    $scope.searchChange = function (force) {
        $scope.showDetails = false;
        $scope.data = {};
        if (!force && (!$scope.searchString || $scope.searchString.length < 3)) {
            $scope.listItems = []
        }  else {
            var searchString = $scope.searchString;
            var searchType = $scope.searchType;
            if (force) {
                searchString = "_____";
                searchType = "_____";
            }

            $scope.baseGoalGet(" - Searching...", $scope.apiSearchUrl + searchString + "/" + searchType + "/").
                baseGoalComplete(function (data) {
                    $scope.listItems = data;
                    $scope.showSearchResults = (data.length > 0);
                })
        }
    };
    $scope.loadItem = function (key) {
        $scope.baseGoalGet(" - Loading...", $scope.apiItemUrl + key + "/").
            baseGoalComplete(function (data) {
                $scope.listItems = [];
                $scope.showSearchResults = false;
                $scope.showDetails = true;
                $scope.data = data;
                if ( !data == null || data == "null")
                    alert("No information returned, possible defect, please report to administrator.")
        })
    };

    if ($routeParams.key && $routeParams.key != -1)
        $scope.loadItem($routeParams.key);
}