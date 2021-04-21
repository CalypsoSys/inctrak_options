function PeriodCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.periodTemplate = '/common/periods.html';

    $scope.delPeriod = function (period) {
        var index = $scope.data.Periods.indexOf(period);
        $scope.data.Periods.splice(index, 1);
    };

    $scope.addPeriod = function () {
        $scope.data.Periods.forEach(function (item, key) {
            item.EVEN_OVER_N = 0;
        });
        $scope.data.Periods.push({ GroupKeyCheck: $scope.GroupKeyCheck, EVEN_OVER_N: 0 })
    };

    $scope.hideAmount = function (period) {
        return parseInt(period.EVEN_OVER_N) == 1;
    }

    $scope.hideIncrement = function (period) {
        return parseInt(period.EVEN_OVER_N) == 2;
    }

    $scope.validatePeriods = function (periods) {
        var error = "";
        for (var i = 0; i < $scope.data.Periods.length; i++) {
            var period = $scope.data.Periods[i];
            period.ORDER = i;

            var periodError = "";
            if (!period.PERIOD_TYPE_FK)
                periodError += "Period Duration ";
            if (!period.AMOUNT_TYPE_FK)
                periodError += "Amount Type ";
            if (!period.AMOUNT && period.EVEN_OVER_N != 1)
                periodError += "Amount ";
            else if (period.EVEN_OVER_N == 1)
                period.AMOUNT = 0;
            if (!period.INCREMENTS && period.EVEN_OVER_N != 2)
                periodError += "Number of Periods ";
            else if (period.EVEN_OVER_N == 2)
                period.INCREMENTS = 0;
            if (!period.PERIOD_AMOUNT)
                periodError += "Period Amount ";
            periods.push(period);
            if (periodError)
                error += "Period " + (i + 1) + " has invalid (" + periodError + ")\r\n";
        }

        return error;
    }
}