$(function () {
	$.ajax({
		url: '/Admin/AdminAccount/GetRole/',
		method: 'GET',
		success: function (data) {
			//console.log("gọi được");
			//console.log("staff", data);
			if (data.staff != null) {
				if (data.staff.vtId == 2) {
					var idPaymentmethod = $('#paymentmethod').data('id');
					var idAccountmanagement = $('#accountmanagement').data('id');
					var idRole = $('#role').data('id');
					$('#paymentmethod').css('display', 'none');
					$('#accountmanagement').css('display', 'none');
					$('#role').css('display', 'none');
					var imgElement = document.getElementById("avtuser");
					var imagePath = '/images/' + data.info.ndHinhanh;
					console.log("hinh anh", imagePath);
					imgElement.src = imagePath;
				}
			}
			/*console.log("admin", data.admin);*/
			if (data.admin != null) {
				if (data.admin.vtId == 1) {
					console.log("admin");
					var imgElement = document.getElementById("avtuser");
					var imagePath = '/images/' + data.info.ndHinhanh;
					console.log("hinh anh", imagePath);
					imgElement.src = imagePath;
				}
			}
			
			
		},
		error: function (error) {
			console.log("Lỗi trong yêu cầu AJAX: ", error);
		}
	});
});