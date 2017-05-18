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

    $scope.selectedYear = 0;
    $scope.selectedYearLabel = '';
    $scope.selectedMonth = 1;
    $scope.selectedMonthLabel = '';
    $scope.shownMonths = [];


    $scope.yearSelected = function () {
        if (new Date().getFullYear() == $scope.selectedYear) {
            $scope.shownMonths = $scope.months.slice(0, new Date().getMonth() + 1);
            $scope.selectedMonth = $scope.shownMonths.length;
            $scope.selectedMonthLabel = $scope.months[$scope.shownMonths.length];
        } else {
            $scope.shownMonths = $scope.months;
            $scope.selectedMonth = 12;
            $scope.selectedMonthLabel = $scope.months[12];
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

    $scope.totalRevenue = 0;


    $scope.fvrGraphValues = [];
    $scope.fvrGraph = null;

    $scope.getGraphValues = function () {
        $.getJSON('http://am-bvs-am-bvs-stage.azurewebsites.net/assetmarkBAT/getvaluationmetrics?pagr=' + $scope.pagr.value + '&pm=' + $scope.pm.value + '&vmi=' + $scope.vmi.value, function (data) {
            console.log(data);
            var graphValues = [];
            graphValues.push([data.currentmin, data.currentmax], [data.calculatedmin, data.calculatedmax]);
            $scope.fvrGraph.series[0].setData(graphValues);
        });
    };

    $scope.updateGraph = function () {
        $scope.getGraphValues();
    };

    $scope.initGraph = function () {

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
                categories: ['Current Value', 'Optimized Value']
            },

            tooltip: {
                enabled: false,
            },

            plotOptions: {
                columnrange: {
                    dataLabels: {
                        enabled: true,
                        formatter: function () {
                            return '$' + this.y;
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

    $scope.myp1 = {
        value: 5
    };
    $scope.myp2 = {
        value: 5
    };
    $scope.myp3 = {
        value: 5
    };
    $scope.myp4 = {
        value: 5
    };
    $scope.myp5 = {
        value: 5
    };

    $scope.myb1 = {
        value: 5
    };
    $scope.myb2 = {
        value: 5
    };
    $scope.myb3 = {
        value: 5
    };
    $scope.myb4 = {
        value: 5
    };
    $scope.myb5 = {
        value: 5
    };

    $scope.oyo1 = {
        value: 5
    };
    $scope.oyo2 = {
        value: 5
    };
    $scope.oyo3 = {
        value: 5
    };
    $scope.oyo4 = {
        value: 5
    };
    $scope.oyo5 = {
        value: 5
    };

    $scope.eyt1 = {
        value: 5
    };
    $scope.eyt2 = {
        value: 5
    };
    $scope.eyt3 = {
        value: 5
    };
    $scope.eyt4 = {
        value: 5
    };
    $scope.eyt5 = {
        value: 5
    };


    $scope.pagr = {
        value: 6.5,
        options: {
            floor: 0,
            ceil: 25.0,
            step: 0.1,
            precision: 1,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            hidePointerLabels: true,
            onEnd: $scope.updateGraph
        }
    };

    $scope.pagrComp = {
        minValue: 6.2,
        maxValue: 11.5,
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
        value: 16,
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
        minValue: 20,
        maxValue: 25,
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
        value: 210,
        options: {
            floor: 0,
            ceil: 1000,
            step: 1,
            showTicks: false,
            showTicksValues: false,
            hideLimitLabels: true,
            hidePointerLabels: true,
            onEnd: $scope.updateGraph
        }
    };

    $scope.vmiComp = {
        minValue: 700,
        maxValue: 900,
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

    $scope.filterCurrency = function (model) {
        console.log(value);
        var filteredValue = value.replace(/[$,.]/g, '');
        return filteredValue;
    };
});

$(function () {
    $('select:not(.exclude)').each(function (e) {
        var select = $(this);
        var dropdown = '';

        select.wrap('<div class="fancy-form-control"></div>');

        dropdown += '<ul class="fancy-form-select">';
        select.find('option').each(function () {
            var option = $(this);
            var list = '';

            console.log(option[0].selected);
            if (option[0].selected) {
                list += '<li data-value="' + option[0].value + '" class="active"><span>' + option[0].text + '</span></li>';
            } else {
                list += '<li data-value="' + option[0].value + '"><span>' + option[0].text + '</span></li>';
            }

            console.log(option);

            dropdown += list;
        });
        dropdown += '</ul>';

        $(dropdown).insertAfter(select);

        //select.parent().find('li[data-value="Other"]').append(select.parent().siblings('input[type="text"]'));
        //select.parent().find('li[data-value="Other"]').append('<span class="fancy-close">close</span>');

    });

    $('body .fancy-form-select').each(function () {
        var select = $(this);

        select.on('click', function (e) {
            //e.preventDefault();
            //if(!select.hasClass('expand')){
            //  select.addClass('expand');
            //}
        });

        select.find('li').on('click', function (e) {
            //e.preventDefault();
            var item = $(this);

            if (select.hasClass('expand')) {
                //if(item.data('value') != 'Other'){
                select.removeClass('expand');
                //}
            } else {
                select.addClass('expand');
            }

            if (item.data('value') == 'Other') {
                select.parent().siblings('input[type="text"]').removeClass('hide');
            } else {
                select.parent().siblings('input[type="text"]').addClass('hide');
            }

            item.siblings('li').removeClass('active');
            item.addClass('active');
            select.prev().find('option').removeAttr('selected');
            select.prev().find('option').filter('[value="' + item.data('value') + '"]').attr('selected', true);
        });
    });

});