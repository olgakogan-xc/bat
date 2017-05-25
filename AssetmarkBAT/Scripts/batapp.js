var $scope;
var app = angular.module('batApp', ['rzModule', 'ui.bootstrap', 'ui.utils.masks']);

app.run(function (RzSliderOptions) {
    RzSliderOptions.options({
        ceil: 10,
        floor: 0,
        showTicks: true,
        showTicksValues: true,
    });
});

app.controller('MainCtrl', function ($scope, $rootScope, $timeout, $uibModal) {
    $scope.practiceTypeRadio = 0;
    $scope.affiliationModeRadio = 0;
    $scope.firmTypeRadio = 0;

    $scope.months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    $scope.currentYear = new Date().getFullYear();

    $scope.selectedYear = 0;
    $scope.selectedYearLabel = '';
    $scope.selectedMonth = -1;
    $scope.selectedMonthLabel = '';
    $scope.shownMonths = [];

    $scope.nonAdvisors = { number: null, validity: true };
    $scope.advisors = { number: null, validity: true };

    $scope.selectYear = function (year) {
        console.log(year);
        if (year === '' || year === 'Previous Year') {
            $scope.selectedYear = 'Previous Year';
        } else {
            $scope.selectedYear = 'YTD ' + $scope.currentYear;
        }
    };

    $scope.yearSelected = function () {
        //if(new Date().getFullYear() == $scope.selectedYear){
        if ($scope.selectedYear == 'YTD ' + $scope.currentYear) {
            $scope.shownMonths = $scope.months.slice(0, new Date().getMonth());
            if ($scope.selectedMonth < 0) {
                $scope.selectedMonth = $scope.shownMonths.length;
            } else if ($scope.selectedMonth > $scope.shownMonths.length) {
                $scope.selectedMonth = $scope.shownMonths.length;
            }
            console.log($scope.shownMonths.length);
            console.log($scope.selectedMonth);
            $scope.selectedMonthLabel = $scope.months[$scope.shownMonths.length];
        } else {
            $scope.shownMonths = $scope.months;
            $scope.selectedMonth = 12;
            $scope.selectedMonthLabel = $scope.months[11];
        }
    };

    $scope.expandPracticeType = false;
    $scope.expandPracticeTypeSelect = function () {
        if ($scope.expandPracticeType) {
            $scope.expandPracticeType = false;
        } else {
            $scope.expandPracticeType = true;
        }
    };

    $scope.expandAffiliationMode = false;
    $scope.expandAffiliationModeSelect = function () {
        if ($scope.expandAffiliationMode) {
            $scope.expandAffiliationMode = false;
        } else {
            $scope.expandAffiliationMode = true;
        }
    };

    $scope.expandFirmType = false;
    $scope.expandFirmTypeSelect = function () {
        if ($scope.expandFirmType) {
            $scope.expandFirmType = false;
        } else {
            $scope.expandFirmType = true;
        }
    };

    $scope.practiceTypeOtherChange = function (index) {
        $scope.practiceTypeRadio = index;
    };

    $scope.setPracticeType = function (type) {
        if (type != '') {
            $scope.practiceType = 'Other';
        }
    };
    $scope.setAffiliationMode = function (type) {
        if (type != '') {
            $scope.affiliationMode = 'Other';
        }
    };
    $scope.setFirmType = function (type) {
        if (type != '') {
            $scope.firmType = 'Other';
        }
    };

    $scope.totalRevenue = 0;

    $scope.fvrGraphValues = [];
    $scope.fvrGraph = null;

    $scope.recalculate = false;

    $scope.getGraphValues = function () {
        $.getJSON('/assetmarkBAT/getvaluationmetrics?pagr=' + $scope.pagr.value + '&pm=' + $scope.pm.value + '&vmi=' + $scope.vmi.value + '&recalculate=' + $scope.recalculate, function (data) {
            //$.getJSON('optimizer.json', function(data){
            var graphValues = [];

            graphValues.push([data.currentmin, data.currentmax], [data.calculatedmin, data.calculatedmax]);

            $scope.fvrGraph.series[0].setData(graphValues);

            $scope.pagrComp.minValue = data.top_pagr_min;
            $scope.pagrComp.maxValue = data.top_pagr_max;

            $scope.pmComp.minValue = data.top_pm_min;
            $scope.pmComp.maxValue = data.top_pm_max;

            $scope.vmiComp.minValue = data.top_vmi_min;
            $scope.vmiComp.maxValue = data.top_vmi_max;

            $scope.profitAnnualized = data.profitannualized;

            if (!$scope.recalculate) {
                $scope.pagr.value = data.pagr * 100;
                $scope.pm.value = data.pm;
                $scope.vmi.value = data.vmi;

                $scope.pagr.valueOg = data.pagr * 100;
                $scope.pm.valueOg = data.pm;
                $scope.vmi.valueOg = data.vmi;
            }

            $scope.operatingProfit = data.operatingprofit;

            $scope.$broadcast('rzSliderForceRender');
        });
    };

    $scope.updateGraph = function () {
        $scope.recalculate = true;
        $scope.getGraphValues();
    };

    $scope.categories = ['Current Value', 'Optimized Value'];

    $scope.initGraph = function () {
        var noData = false;

        $scope.fvrGraph = Highcharts.chart('optimizerGraph', {

            chart: {
                type: 'columnrange',
                inverted: false
            },

            yAxis: {
                labels: {
                    formatter: function () {
                        return '$' + this.value;
                    }
                },
                title: {
                    enabled: false
                }
            },

            xAxis: {
                categories: $scope.categories
            },

            tooltip: {
                enabled: false,
            },

            plotOptions: {
                column: {
                    colorByPoint: true
                },
                columnrange: {
                    dataLabels: {
                        enabled: true,
                        formatter: function () {
                            if (this.point.high === 0 && this.point.low === 0) {
                                if (noData) {
                                    return '';
                                } else {
                                    noData = true;
                                }
                            }

                            if (this.point.high === 0 && this.point.low === 0) {
                                return 'No Data';
                            } else {
                                return '$' + this.y;
                            }
                        }
                    }
                }
            },

            legend: {
                enabled: false
            },

            series: [{
                data: [[0, 0], [0, 0]]
            }]

        });

        $scope.getGraphValues();
    };

    $scope.calculatedTotalScore = 500;
    $scope.vmiSliderChanged = false;

    $scope.vmiInitSliderChanged = function (status) {
        if (status === 'True') {
            $scope.vmiSliderChanged = true;
        } else {
            $scope.vmiSliderChanged = false;
        }
    };

    $scope.calculateScore = function () {
        var myp = $scope.myp1.value + $scope.myp2.value + $scope.myp3.value + $scope.myp4.value + $scope.myp5.value;
        var myb = $scope.myb1.value + $scope.myb2.value + $scope.myb3.value + $scope.myb4.value + $scope.myb5.value;
        var oyo = $scope.oyo1.value + $scope.oyo2.value + $scope.oyo3.value + $scope.oyo4.value + $scope.oyo5.value;
        var eyt = $scope.eyt1.value + $scope.eyt2.value + $scope.eyt3.value + $scope.eyt4.value + $scope.eyt5.value;

        $scope.calculatedTotalScore = (myp + myb + oyo + eyt) * 5;
    };


    $scope.updateScore = function () {
        $scope.calculateScore();
        $scope.vmiSliderChanged = true;
    };

    $scope.vmiSliders = {
        options: {
            onEnd: $scope.updateScore
        }
    };

    $scope.pagr = {
        value: 0,
        options: {
            floor: 0,
            ceil: 25.0,
            step: 1,
            precision: 1,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            hidePointerLabels: true,
            onEnd: $scope.updateGraph
        }
    };

    $scope.pagrComp = {
        minValue: 0,
        maxValue: 0,
        options: {
            floor: 0,
            ceil: 25.0,
            step: 0.1,
            precision: 1,
            disabled: true,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            ticksArray: [0, 5, 10, 15, 20, 25],
            hidePointerLabels: true,
            draggableRange: true
        }
    };

    $scope.pagrTicks = {
        minValue: 0,
        options: {
            floor: 0,
            ceil: 25.0,
            step: 0.1,
            precision: 1,
            disabled: true,
            showTicks: true,
            showTicksValues: false,
            hideLimitLabels: true,
            stepsArray: [
              { value: 0, legend: '0' },
              { value: 1 },
              { value: 2 },
              { value: 3 },
              { value: 4 },
              { value: 5, legend: '5' },
              { value: 6 },
              { value: 7 },
              { value: 8 },
              { value: 6 },
              { value: 10, legend: '10' },
              { value: 11 },
              { value: 12 },
              { value: 13 },
              { value: 14 },
              { value: 15, legend: '15' },
              { value: 16 },
              { value: 17 },
              { value: 18 },
              { value: 19 },
              { value: 20, legend: '20' },
              { value: 21 },
              { value: 22 },
              { value: 23 },
              { value: 24 },
              { value: 25, legend: '25' }
            ],
            hidePointerLabels: true,
            draggableRange: true
        }
    };

    $scope.pm = {
        value: 0,
        options: {
            floor: 0,
            ceil: 40,
            step: 1,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            hidePointerLabels: true,
            onEnd: $scope.updateGraph
        }
    };

    $scope.pmComp = {
        minValue: 0,
        maxValue: 0,
        options: {
            floor: 0,
            ceil: 40,
            step: 1,
            disabled: true,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            ticksArray: [0, 10, 20, 30, 40],
            hidePointerLabels: true,
            draggableRange: true
        }
    };

    $scope.pmTicks = {
        minValue: 0,
        options: {
            floor: 0,
            ceil: 25,
            step: 0.1,
            disabled: true,
            showTicks: true,
            showTicksValues: false,
            hideLimitLabels: true,
            stepsArray: [
              { value: 0, legend: '0' },
              { value: 2 },
              { value: 3 },
              { value: 4 },
              { value: 5 },
              { value: 10, legend: '10' },
              { value: 12 },
              { value: 14 },
              { value: 16 },
              { value: 18 },
              { value: 20, legend: '20' },
              { value: 22 },
              { value: 24 },
              { value: 26 },
              { value: 28 },
              { value: 30, legend: '30' },
              { value: 32 },
              { value: 34 },
              { value: 36 },
              { value: 38 },
              { value: 40, legend: '40' }
            ],
            hidePointerLabels: true,
            draggableRange: true
        }
    };

    $scope.vmi = {
        value: 0,
        options: {
            floor: 0,
            ceil: 1000,
            step: 50,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            hidePointerLabels: true,
            onEnd: $scope.updateGraph
        }
    };

    $scope.vmiComp = {
        minValue: 0,
        maxValue: 0,
        options: {
            floor: 0,
            ceil: 1000,
            step: 1,
            disabled: true,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            ticksArray: [0, 200, 400, 600, 800, 1000],
            hidePointerLabels: true,
            draggableRange: true
        }
    };

    $scope.vmiTicks = {
        minValue: 0,
        options: {
            floor: 0,
            ceil: 1000,
            step: 1,
            disabled: true,
            showTicks: true,
            showTicksValues: false,
            hideLimitLabels: true,
            stepsArray: [
              { value: 0, legend: '0' },
              { value: 50 },
              { value: 100 },
              { value: 150 },
              { value: 200, legend: '200' },
              { value: 250 },
              { value: 300 },
              { value: 350 },
              { value: 400, legend: '400' },
              { value: 450 },
              { value: 500 },
              { value: 550 },
              { value: 600, legend: '600' },
              { value: 650 },
              { value: 700 },
              { value: 750 },
              { value: 800, legend: '800' },
              { value: 850 },
              { value: 900 },
              { value: 950 },
              { value: 1000, legend: '1000' }
            ],
            hidePointerLabels: true,
            draggableRange: true
        }
    };

    $scope.resetVmiSliders = function () {
        $scope.pagr.value = $scope.pagr.valueOg;
        $scope.pm.value = $scope.pm.valueOg;
        $scope.vmi.value = $scope.vmi.valueOg;

        $scope.updateGraph();
    };

    $scope.filterCurrency = function (model) {
        console.log(value);
        var filteredValue = value.replace(/[$,.]/g, '');
        return filteredValue;
    };
});

app.directive('isNonAdvisors', function () {
    return {
        require: 'ngModel',
        link: function (scope) {
            scope.$watch('nonAdvisors.number', function (newValue, oldValue) {
                console.log('watch');
                var arr = String(newValue).split("");
                if (arr.length === 0) return;
                if (arr.length === 1 && (arr[0] == '-' || arr[0] === '.')) return;
                if (arr.length === 2 && newValue === '-.') return;
                if (isNaN(newValue)) {
                    scope.nonAdvisors.number = oldValue;
                }
            });
        }
    };
});

app.directive('isAdvisors', function () {
    return {
        require: 'ngModel',
        link: function (scope) {
            scope.$watch('advisors.number', function (newValue, oldValue) {
                console.log('watch');
                var arr = String(newValue).split("");
                if (arr.length === 0) return;
                if (arr.length === 1 && (arr[0] == '-' || arr[0] === '.')) return;
                if (arr.length === 2 && newValue === '-.') return;
                if (isNaN(newValue)) {
                    scope.advisors.number = oldValue;
                }
            });
        }
    };
});



$(function () {
    $('select:not(.exclude)').each(function () {
        var select = $(this);

        if (select[0].options[0].text === '') {
            console.log(select.prop('selectedIndex', 1));
        }
    });

    $('select:not(.exclude)').each(function (e) {
        var select = $(this);
        var dropdown = '';

        select.wrap('<div class="fancy-form-control"></div>');

        dropdown += '<ul class="fancy-form-select">';
        select.find('option').each(function () {
            var option = $(this);
            var list = '';

            if (option[0].selected) {
                list += '<li data-value="' + option[0].value + '" class="active"><span>' + option[0].text + '</span></li>';
            } else {
                list += '<li data-value="' + option[0].value + '"><span>' + option[0].text + '</span></li>';
            }

            dropdown += list;
        });
        dropdown += '</ul>';

        $(dropdown).insertAfter(select);
    });

    $('body .fancy-form-select').each(function () {
        var select = $(this);

        select.find('li').on('click', function (e) {
            e.preventDefault();
            var item = $(this);

            if (select.hasClass('expand')) {
                select.removeClass('expand');
            } else {
                select.addClass('expand');
            }

            if (item.data('value') == 'Other') {
                select.parent().siblings('input[type="text"]').removeClass('hide').focus();
            } else {
                select.parent().siblings('input[type="text"]').addClass('hide');
            }

            item.siblings('li').removeClass('active');
            item.addClass('active');
            select.prev().find('option').removeAttr('selected');
            select.prev().find('option').filter('[value="' + item.data('value') + '"]').attr('selected', true);
        });
    });

    $('button[type="submit"').on('click', function (e) {
        $('#form').submit();
    });

});