function DepartmentsCtrl($scope, $cookies, $http, $routeParams, $location) {
    $scope.$emit('newPageLoaded', {
        title: 'Company Departments',
        description: 'Create your Company Departments.',
        keywords: 'sprint development goal startup employee'
    });

    $scope.showDepartments = true;
    $scope.showDetails = false;
    $scope.listItems = [];
    $scope.departmentKey = null;
    BaseGoalCtrl.call(this, $scope, $cookies, $http, $routeParams, $location);

    $scope.baseGoalGet(" - Loading...", "/api/company/departments/").baseGoalComplete(function (data) {
        $scope.breadCrumbs = [];
        $scope.listItems = data;
    });

    $scope.loadDepartment = function (key) {
        var uuid = $cookies.UUID;
        $scope.departmentKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Department - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalGet(" - Loading...", "/api/company/department/" + $scope.departmentKey + "/" + uuid + "/").
                baseGoalComplete(function (data) {
                    if (data.success) {
                        $scope.breadCrumbs =[{url : "#/company_departments/" + Math.random() + "/", name: "> Department"}];
                        $scope.listItems =[];
                        $scope.showDepartments = false;
                        $scope.showDepartment = true;
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
            $scope.save_message = "Please register or login to set Department - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            var error = "";
            var department = $scope.data.Department;
            var departmentError = "";
            if (!department.NAME)
                departmentError += "Department Name ";
            if ( departmentError )
                error += "Department has invalid (" + departmentError + ")\r\n";

            if ( error ) {
                $scope.save_message = error;
                $scope.showDialog = true;
            } else {
                $scope.preBusy();
                $http.post("/api/company/department/", { Key: $scope.departmentKey, UUID: uuid, Data: department }).
                    success(function (data) {
                        $scope.save_message = data.message;
                        $scope.showDialog = true;
                        $scope.postBusy();
                        $scope.loadDepartment(data.key);
                    }).error(function (data) {
                        $scope.save_message = "An error occured saving your Department, try again?";
                        $scope.showDialog = true;
                        $scope.postBusy();
                    });
            }
        }
    };

    $scope.removeDepartment = function (key) {
        var uuid = $cookies.UUID;
        $scope.departmentKey = key;

        if (!uuid || uuid == "not set") {
            $scope.save_message = "Please register or login to set Department - thx";
            $scope.navigateUrl = "/login";
            $scope.showDialog = true;
        } else {
            $scope.baseGoalDelete(" - Loading...", "/api/company/department/" +$scope.departmentKey + "/" +uuid + "/").
                baseGoalRemoved(function (data) {
                    if (data.success) {
                        $scope.listItems = data.Departments;
                        $scope.breadCrumbs =[];
                        $scope.showDepartments = true;
                        $scope.showDepartment = false;
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

