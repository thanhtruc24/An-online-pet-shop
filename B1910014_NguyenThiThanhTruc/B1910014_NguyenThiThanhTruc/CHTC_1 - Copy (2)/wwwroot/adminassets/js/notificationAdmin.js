document.addEventListener("DOMContentLoaded", function () {
    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/NotificationHub")
        .build();
    hubConnection.start().then(function () {
        console.log("Kết nối thành công!");
        var count = 0;
        $('#icon').click(function () {
            var idpopover = $('#popover').data('id')
            console.log("vào")
            if ($('#popover').css('display') == "block") {
                $('#popover').css('display', 'none');

            }
            else {
                $('#popover').css('display', 'block');
                $('#popover').css('max-height', '435px');

            }
        });
        hubConnection.on("SendMessage", function (user, message) {
            console.log("đêm", count)
            // Xử lý thông báo khi đặt hàng thành công
            if (count == 0) {
                count = 1;
                //console.log(message + user);
                //console.log("so luong", count); 
            }
            else {
                count++;
                //console.log(message + user);
                // console.log("so luong", count); 
            }
            var jsonData = JSON.stringify(user);
            //console.log("data", jsonData);
            $.ajax({
                type: "POST", // Loại request (có thể sử dụng GET hoặc POST tùy theo yêu cầu của bạn)
                url: '/Admin/ThongBaos/CheckoutNotification', // URL của endpoint trên server
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: jsonData,
                success: function (data) {
                    //console.log("thong bao", data.dsthongbao);
                    var notificationContainer = document.getElementById("popover");
                    $("#popover").empty();
                    for (let i = 0; i < data.dsthongbao.length; i++) {
                        var title = data.dsthongbao[i].tbTieude;
                        var content = data.dsthongbao[i].tbNoidung;

                        var formattedTime = moment(data.dsthongbao[i].tbThoigian).format('DD/MM/YYYY HH:mm');
                        var time = formattedTime;
                        var avt = data.dsthongbao[i].tbAvt;
                        console.log("format time", time)

                        // Tạo phần tử thông báo
                        var notificationElement = document.createElement("div");
                        notificationElement.className = "popover-container";
                        var container = document.createElement("div");
                        container.style.maxHeight = "435px"; // Đặt chiều cao tối đa cho phần tử cha
                        container.style.overflow = "auto"; // Tạo thanh cuộn khi cần
                        container.appendChild(notificationElement);
                        if (data.dsthongbao[i].TbTrangthai == 0) {
                            var innerHtml = `
                               
                                                                <img class="rounded-circle shadow-1-strong me-3" src="/images/${avt}" id="avt-noti" width="40" height="40" />
                                                                <div style="width:300px">
                                                                    <div class="title-noti noti">${title}</div>
                                                                    <div class="content-noti noti">${content}</div>
                                                                    <div class="time-noti noti">${time}</div>
                                                                </div>
                                                                <span class="material-symbols-outlined status-noti bg-info bg-gradient">
                                                                    radio_button_unchecked
                                                                </span>

                                                        `;

                            notificationElement.innerHTML = innerHtml;

                            // Thêm phần tử thông báo vào container
                            notificationContainer.appendChild(notificationElement);
                        }
                        else {
                            var innerHtml = `

                                                                <img class="rounded-circle shadow-1-strong me-3" src="/images/${avt}" id="avt-noti" width="40" height="40" />
                                                                <div style="width:300px">
                                                                    <div class="title-noti noti">${title}</div>
                                                                    <div class="content-noti noti">${content}</div>
                                                                    <div class="time-noti noti">${time}</div>
                                                                </div>
                                                                <span class="material-symbols-outlined status-noti bg-gradient">
                                                                    radio_button_unchecked
                                                                </span>

                                                        `;

                            notificationElement.innerHTML = innerHtml;

                            // Thêm phần tử thông báo vào container
                            notificationContainer.appendChild(notificationElement);
                        }




                        //console.log("tieu de", data.dsthongbao[i])
                        //$('.title-noti').text(data.dsthongbao[i].tbTieude);
                        //$('.content-noti').text(data.dsthongbao[i].tbNoidung);
                        //$('.time-noti').text(data.dsthongbao[i].tbThoigian);
                    }
                    $('#countnotifications').text(data.count);
                }
            })

        });
    }).catch(function (err) {
        console.error(err.toString());
    });

    //đếm lại thông báo

    $(function () {
        var count = 0;
        $.ajax({
            url: '/Admin/Home/CountNotification',
            method: 'GET',
            success: function (data) {
                console.log(data)

                $('#countnotifications').text(data.count);
            }
        });
    });

    //đã xem thông báo
    $('#icon').click(function () {
        $.ajax({
            url: '/Admin/Home/SetStatus',
            method: 'GET',
            success: function (data) {
                var notificationContainer = document.getElementById("popover");
                $("#popover").empty();
                for (let i = 0; i < data.aftersetstatus.length; i++) {
                    var title = data.aftersetstatus[i].tbTieude;
                    var content = data.aftersetstatus[i].tbNoidung;
                    //var time = data.aftersetstatus[i].tbThoigian;
                    var avt = data.aftersetstatus[i].tbAvt;
                    var formattedTime = moment(data.aftersetstatus[i].tbThoigian).format('DD/MM/YYYY HH:mm');
                    var time = formattedTime;

                    console.log("format time", time)
                    // Tạo phần tử thông báo
                    var notificationElement = document.createElement("div");
                    notificationElement.className = "popover-container";
                    var container = document.createElement("div");
                    container.style.maxHeight = "435px"; // Đặt chiều cao tối đa cho phần tử cha
                    container.style.overflow = "auto"; // Tạo thanh cuộn khi cần
                    container.appendChild(notificationElement);
                    if (data.aftersetstatus[i].TbTrangthai == 0) {
                        var innerHtml = `

                                                            <img class="rounded-circle shadow-1-strong me-3" src="/images/${avt}" id="avt-noti" width="40" height="40" />
                                                            <div style="width:300px">
                                                                <div class="title-noti noti">${title}</div>
                                                                <div class="content-noti noti">${content}</div>
                                                                <div class="time-noti noti">${time}</div>
                                                            </div>
                                                            <span class="material-symbols-outlined status-noti bg-info bg-gradient">
                                                                radio_button_unchecked
                                                            </span>

                                                    `;

                        notificationElement.innerHTML = innerHtml;

                        // Thêm phần tử thông báo vào container
                        notificationContainer.appendChild(notificationElement);
                    }
                    else {
                        var innerHtml = `

                                                            <img class="rounded-circle shadow-1-strong me-3" src="/images/${avt}" id="avt-noti" width="40" height="40" />
                                                            <div style="width:300px">
                                                                <div class="title-noti noti">${title}</div>
                                                                <div class="content-noti noti">${content}</div>
                                                                <div class="time-noti noti">${time}</div>
                                                            </div>
                                                            <span class="material-symbols-outlined status-noti bg-gradient">
                                                                radio_button_unchecked
                                                            </span>

                                                    `;

                        notificationElement.innerHTML = innerHtml;

                        // Thêm phần tử thông báo vào container
                        notificationContainer.appendChild(notificationElement);
                    }




                    //console.log("tieu de", data.dsthongbao[i])
                    //$('.title-noti').text(data.dsthongbao[i].tbTieude);
                    //$('.content-noti').text(data.dsthongbao[i].tbNoidung);
                    //$('.time-noti').text(data.dsthongbao[i].tbThoigian);
                }
                console.log(data.aftersetstatus)
                count = 0;
                console.log("đang mở")
                $('#countnotifications').text(count);
            }
        });


    });
});
