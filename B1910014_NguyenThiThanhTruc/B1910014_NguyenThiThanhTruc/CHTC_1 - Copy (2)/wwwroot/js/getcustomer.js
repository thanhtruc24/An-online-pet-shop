$(function () {
	$.ajax({
		url: '/Accounts/GetCustomer/',
		method: 'GET',
		success: function (data) {
			console.log("người dùng", data);
			if (data.nguoiDung != null) {
				$('#userName').text(data.nguoiDung.ndHoten);
				//var imgElement = document.getElementById("avtcustomer");
				//var imagePath = '/images/' + data.nguoiDung.ndHinhanh;
				//console.log("hinh anh", imagePath);
				//imgElement.src = imagePath;
			}
		}
	});
});