(function () {
    'use strict';
    var app = angular.module('app', [
        'infinite-scroll'
    ]);

    app.controller('searchController', [
        "$scope", "$http", function ($scope, $http) {
            $scope.term = "";

            $scope.items = [];

            $scope.currentPage = 1;
            $scope.busy = false;
            $scope.noResult = false;

            $scope.search = function () {
                if ($scope.term.trim() === "") return;
                $scope.busy = true;
                $scope.noResult = false;
                $http.get("/home/search?q=" + $scope.term + "&page=" + $scope.currentPage).then(function (response) {
                    var result = response.data;
                    $scope.numberPerPage = result.numberPerPage;

                    $scope.resultsShowing = result.NumberPerPage * result.CurrentPage;
                    $scope.totalResult = result.TotalCount;
                    $scope.items = result.Items;
                    if (result.Items.length > 0) {
                        $scope.noResult = false;
                    } else {
                        $scope.noResult = true;
                    }
                    $scope.busy = false;
                });
            }

            $scope.$watch('term', function () {
                $scope.currentPage = 1;
                $scope.search();
            });

            $scope.nextPage = function () {
                var pageCount = $scope.totalResult / $scope.numberPerPage + ($scope.totalResult % $scope.numberPerPage > 0 ? 1 : 0);
                if ($scope.currentPage >= pageCount) {
                    return;
                }
                $scope.currentPage += 1;
                $scope.search();
            }

            $scope.previousPage = function () {
                if ($scope.currentPage <= 1) {
                    return;
                }
                $scope.currentPage -= 1;
                $scope.search();
            }
        }
    ]);


})();