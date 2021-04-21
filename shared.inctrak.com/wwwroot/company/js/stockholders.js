function StockHoldersCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Stock Holders',
        description: 'Create your Company Stock Holders.',
        keywords: 'stock options grant plan startup employee benefits'
    });

    $scope.apiSearchUrl = '/api/company/stockholders/';
    $scope.apiItemUrl = '/api/company/stockholder/';
    SearchLoadOptioneePlanCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.showStockHolders = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.stockHolderKey = null;

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

    $scope.loadStockHolder = function (key) {
        var uuid = $cookies.UUID;
        $scope.stockHolderKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set stock holder - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanGet(" - Loading...", "/api/company/stockholder/" + $scope.stockHolderKey + "/" + uuid + "/").
                optioneePlanComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs = [{ url: "#/company_stockholders/" + Math.random() + "/", name: "> Stock Holder" }];
                        $scope.listItems = [];
                        $scope.showStockHolders = false;
                        $scope.showStockHolder = true;
                        $scope.data = data;
                        if (data && data.StockHolder && data.StockHolder.DATE_OF_SALE) {
                            data.StockHolder.DATE_OF_SALE = data.StockHolder.DATE_OF_SALE.split('T')[0];
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
            $scope.save_message = "Please register or login to set stock holders - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var stockHolder = $scope.data.StockHolder;
            var stockHolderError = "";
            if ($scope.participant.selected)
                stockHolder.PARTICIPANT_FK = $scope.participant.selected.PARTICIPANT_PK;

            if (!stockHolder.PARTICIPANT_FK)
                stockHolderError += " Participant ";
            if (!stockHolder.STOCK_CLASS_FK)
                stockHolderError += " Stock Class ";
            if (!stockHolder.DATE_OF_SALE)
                stockHolderError += " Date of Sale ";
            if (!stockHolder.PRICE)
                stockHolderError += " Price ";
            if (!stockHolder.SHARES)
                stockHolderError += " Shares ";

            if (stockHolderError)
                error += "Stock Holder has invalid (" + stockHolderError + ")\r\n";

            if (error) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/stockholder/", { Key: $scope.stockHolderKey, UUID: uuid, Data: stockHolder }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadStockHolder(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your stock holder, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };


    $scope.removeStockHolder = function (key) {
        var uuid = $cookies.UUID;
        $scope.stockHolderKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to remove stock holders - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.optioneePlanDelete(" - Loading...", "/api/company/stockholder/" + $scope.stockHolderKey + "/" + uuid + "/").
                optioneePlanRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.StockHolder;
                        $scope.breadCrumbs = [];
                        $scope.showStockHolders = true;
                        $scope.showStockHolder = false;
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

