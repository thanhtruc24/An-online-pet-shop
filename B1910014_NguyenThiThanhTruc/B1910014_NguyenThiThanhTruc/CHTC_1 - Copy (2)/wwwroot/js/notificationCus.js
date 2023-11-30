document.addEventListener("DOMContentLoaded", function () {
    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/NotificationHub")
        .build();
    hubConnection.start().then(function () {
        console.log("Kết nối thành công!");
        var count = 0;
        $('#iconCus').click(function () {
            var idpopover = $('#popoverCus').data('id')
            console.log("vào")
            if ($('#popoverCus').css('display') == "block") {
                $('#popoverCus').css('display', 'none');

            }
            else {
                $('#popoverCus').css('display', 'block');
                $('#popoverCus').css('max-height', '435px');

            }
        });
        hubConnection.on("SendNotification", function (user, message) {
            if (count == 0) {
                count = 1;
            }
            else {
                count++;
            }
            var jsonData = JSON.stringify(user);
            //console.log("user", user);
            //console.log("message", message);
            $.ajax({
                type: "POST", // Loại request (có thể sử dụng GET hoặc POST tùy theo yêu cầu của bạn)
                url: '/Admin/ThongBaos/GetNotificationCus', // URL của endpoint trên server
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: jsonData,
                success: function (data) {
                    console.log(data);
                    $('#countnotificationsCus').text(data.count);
                    
                }
            });

        });



        $('#iconCus').click(function () {
            $.ajax({
                url: '/Admin/ThongBaos/SetStatus',
                method: 'GET',
                success: function (data) {
                    console.log("data", data)
                    var notificationContainer = document.getElementById("popoverCus");
                    $("#popoverCus").empty();
                    console.log("data", data);
                    console.log("chieu dai", data.aftersetstatus.length);
                    for (let i = 0; i < data.aftersetstatus.length; i++) {
                        var title = data.aftersetstatus[i].tbTieude;
                        var content = data.aftersetstatus[i].tbNoidung;
                        //var time = data.aftersetstatus[i].tbThoigian;
                        var avt = data.aftersetstatus[i].tbAvt;
                        var formattedTime = moment(data.aftersetstatus[i].tbThoigian).format('DD/MM/YYYY HH:mm');
                        var time = formattedTime;

                        /*console.log("format time", time)*/
                        // Tạo phần tử thông báo
                        var notificationElement = document.createElement("div");
                        notificationElement.className = "popover-container-cus";
                        var container = document.createElement("div");
                        container.style.maxHeight = "420px"; // Đặt chiều cao tối đa cho phần tử cha
                        container.style.overflow = "auto"; // Tạo thanh cuộn khi cần
                        container.appendChild(notificationElement);
                        if (data.aftersetstatus[i].TbTrangthai == 0) {
                            var innerHtml = `

                                                            <img class="rounded-circle shadow-1-strong me-3" src="/images/${avt}" id="avt-noti" width="40" height="40" />
                                                            <div style="width:430px">
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




                    }
                    console.log(data.aftersetstatus)
                    count = 0;
                    console.log("đang mở")
                    $('#countnotificationsCus').text(count);
                }
            });


        });

        $(function () {
            var count = 0;
            $.ajax({
                url: '/Admin/ThongBaos/CountNotificationCus',
                method: 'GET',
                success: function (data) {
                    console.log(data)

                    $('#countnotificationsCus').text(data.count);
                }
            });
        });
    });
});
