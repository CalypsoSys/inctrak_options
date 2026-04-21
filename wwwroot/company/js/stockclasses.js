function StockClassesCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Options Stock Classes',
        description: 'Create your COmpany Stock option Stock Classes.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.showStockClasses = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.stockClassKey = null;
    BaseOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.optioneePlanGet(" - Loading...", "/api/company/stockclasses/").optioneePlanComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadStockClass = function (key) {
        var uuid = $cookies.UUID;
        $scope.stockClassKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Stock Classes - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/stockclass/" + $scope.stockClassKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs =[{url : "#/company_stockclasses/" + Math.random() + "/", name: "> Stock Classes"}];
                        $scope.listItems =[];
                        $scope.showStockClasses = false;
                        $scope.showStockClass = true;
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
            $scope.save_message = "Please register or login to set Stock Classes - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var stockClass = $scope.data.StockClass;
            var stockClassError = "";
            if (!stockClass.NAME)
                stockClassError += "Stock Class Name ";
            if (stockClass.TOTAL_SHARES < 1)
                stockClassError += "Round ";
            if ( stockClassError )
                error += "Stock Class has invalid (" + stockClassError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/stockclass/", { Key: $scope.stockClassKey, UUID: uuid, Data: stockClass }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadStockClass(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your Stock Class, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };

    $scope.removeStockClass = function (key) {
        var uuid = $cookies.UUID;
        $scope.stockClassKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Stock Classes - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/stockclass/" +$scope.stockClassKey + "/" +uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.StockClasses;
                        $scope.breadCrumbs =[];
                        $scope.showStockClasses = true;
                        $scope.showStockClass = false;
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

