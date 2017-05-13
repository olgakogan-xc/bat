var app = angular.module('bat', ['rzModule', 'ui.bootstrap', 'ui.utils.masks']);

app.run(function( RzSliderOptions ) {
	RzSliderOptions.options({
		ceil: 10,
		floor: 0,
		showTicks: true,
		showTicksValues: true
	});
});

app.controller('MainCtrl', function($scope, $rootScope, $timeout, $uibModal) {
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
/*
	$scope.numberWith2Decimals = 0.0;
	$scope.percentageWithDefaultDecimals = 0.7654;

	$scope.initializedCpf = '35244457640';
	$scope.initializedCnpj = '13883875000120';
	$scope.initializedCpfCnpj1 = '56338332958';
	$scope.initializedCpfCnpj2 = '23212161000144';

	$scope.defaultMoney = 153.12;
	$scope.negativeMoney = -153.12;
	$scope.moneyStartedWith0 = 0;
	$scope.moneyInitializedWithString = '3.53';

	$scope.initializedPhoneNumber = '3133536767';

	$scope.initializedCep = '30112010';

	$scope.states = ['AC','AL','AM','AP','BA','CE','DF','ES','GO','MA',
		'MG','MS','MT','PA','PB','PE','PI','PR','RJ','RN','RO','RR',
		'RS','SC','SE','SP','TO'];
	$scope.fixedStateIE = '0623079040081';
	$scope.initializedState = 'SP';
	$scope.initializedIE = 'P3588747709710';

	$scope.initializedDateMask = new Date();
	$scope.initializedWithISOStringDateMask = (new Date()).toISOString();
*/
});