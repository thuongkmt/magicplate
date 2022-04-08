var abp = abp || {};
abp.taskIdTimeoutGuideDialog = 0;
abp.closeGuideDialog = function () {
    $('#modal-guide').modal('hide');
    if (abp.taskIdTimeoutGuideDialog > 0)
        clearTimeout(abp.taskIdTimeoutGuideDialog);
    abp.taskIdTimeoutGuideDialog = setTimeout(function () {
        $('#modal-guide').modal('show');
    }, 90 * 1000);
};

(function () {
    // Check if SignalR is defined
    if (!signalR) {
        return;
    }

    // Create namespaces
    abp.signalr = abp.signalr || {};
    abp.signalr.hubs = abp.signalr.hubs || {};
    abp.appPath = "http://localhost:22742/";
    var enablePaymentSound = new Audio("../sound/payment.mp3");
    var errorSound = new Audio("../sound/error.wav");
    var successSound = new Audio("../sound/success.wav");
    abp.signalr.transaction = {};
    var countError = 0;
    var isLoading = false;

    // Configure the connection
    function configureConnection(connection) {
        // Set the common hub
        abp.signalr.hubs.common = connection;

        // Reconnect if hub disconnects
        connection.onclose(function (e) {
            if (e) {
                console.log('Connection closed with error: ' + e);
            } else {
                console.log('Disconnected');
            }

            if (!abp.signalr.autoConnect) {
                return;
            }

            setTimeout(function () {
                abp.signalr.connect();
            }, 5000);
        });

        // Register to updateDishes
        connection.on('updateTransactionInfo', function (transactionInfo) {
            console.log("Updated transaction on UI");
            console.log(transactionInfo);

            if (transactionInfo) {
                const {
                    paymentState: state
                } = transactionInfo;
                paymentState = state;
                handlePaymentStateChange(state);
            }

            abp.closeGuideDialog();
            abp.signalr.transaction = transactionInfo;
            if (!transactionInfo)
                return;

            // Set total plates.
            if (transactionInfo.plateCount != $('#plate-count').text()) {
                $('#plate-count').text(transactionInfo.plateCount);
                $('#plate-count').animateCss('bounce', function () {
                    $('#plate-count').removeClass('bounce');
                });
            }

            if (transactionInfo.discountPercentage > 0) {
                $('#discount-container').show();
                $('#discount-value').text(transactionInfo.discountPercentage.toFixed(0));
            } else {
                $('#discount-container').hide();
            }

            console.log(transactionInfo.paymentState, transactionInfo.customerMessage);

            if (transactionInfo.customerMessage && transactionInfo.customerMessage !== '') {
                // Wait tap card.
                if (transactionInfo.paymentState == 200 || transactionInfo.paymentState == 110) {
                    // Master/Visa
                    if (transactionInfo.paymentType == 104) {
                        $('#message-processing').text('Mastercard Visa Selected. Please tap your card');
                    }

                    // EZ-Link
                    else if (transactionInfo.paymentType == 103) {
                        $('#message-processing').text('Ezlink NetsFlashPay Selected. Please tap your card');
                    }

                    //QR dash
                    else if (transactionInfo.paymentType == 106) {
                        $('#message-processing').text('Please scan QR to pay');
                    }

                    // Konbi Credits
                    else if (transactionInfo.paymentType == 108) {
                        $('#message-processing').text('Please place your membership card');
                    }

                    // Facial Recog
                    else if (transactionInfo.paymentType == 109) {
                        $('#message-processing').text('Please look into the camera device to dectect');
                    }
                }

                else if (transactionInfo.paymentState == 101 && $('#change-payment-method').is(":visible")) {
                }

                else {
                    $('#message-processing').text(transactionInfo.customerMessage);
                    $('#message-processing').removeClass('message-error');
                    $('#message-processing').removeClass('message-success');

                    // Select payment step, Show Blue background
                    if (transactionInfo.customerMessage == "Please Select Payment Method") {
                        background_choose_payment_method();
                    }
                }
            }
            else {
                $('#message-processing').text('To begin, please place tray down');
                $('#message-processing').removeClass('message-error');
                $('#message-processing').removeClass('message-success');
                $('#konbi-credit-msg').removeClass();
                $('#konbi-credit-msg').hide();
                $('#print-receipt').hide();
                $('#change-payment-method').hide();
                setCountDown(0, true);
            }

            // Set show Loading.
            if (transactionInfo.paymentState == 100 && transactionInfo.customerMessage != '') {
                $('#show-loading').show();
                $('#select-payment').hide();
                $('#select-mastercardvisa').hide();
                $('#select-qr-dash').hide();
                $('#select-konbi-credits').hide();
                $('#select-ezlink-netsflashpay').hide();
                background_clear();
            }
            else {
                $('#show-loading').hide();

                if (transactionInfo.paymentState == 101 && $('#change-payment-method').is(":visible")) {
                    $('#select-payment').hide();
                } else {
                    $('#select-payment').show();
                }
                $('#select-mastercardvisa').hide();
                $('#select-qr-dash').hide();
                $('#select-konbi-credits').hide();
                $('#select-ezlink-netsflashpay').hide();
            }
            //Payment successful
            if (transactionInfo.paymentState == 300) {
                $('#message-processing').text('Payment Successful. Thank you');
                $('#message-processing').addClass('message-success');

                if (transactionInfo.paymentType == 108) {
                    // Show Konbi Credit Balance
                    var konbiCreditMsg = 'Balance: $' + transactionInfo.konbiCreditBalance;
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-success message-summary');
                    $('#konbi-credit-msg').show();
                }
                else if(transactionInfo.paymentType==109){
                    // Show Konbi Credit Balance
                    var konbiCreditMsg = ` Username: ${transactionInfo.buyer}, Balance: $ ${transactionInfo.konbiCreditBalance}`;
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-success message-summary');
                    $('#konbi-credit-msg').show();
                }

                $('#print-receipt').show();
                $('#select-payment').hide();

                setCountDown(0, true);
                background_clear();
                successSound.play();
                background_success();
                setTimeout(function () {
                    background_clear();
                }, 3000);
            }

            // error
            if (transactionInfo.paymentState == 400) {
                $('#message-processing').text('Payment Unsuccessful. Please try again or contact a cashier');
                $('#message-processing').addClass('message-error');

                if (transactionInfo.paymentType == 108
                    && transactionInfo.isTokenRetrieved == 1 
                    && transactionInfo.isUserExist == 0) {
                    // Show Konbi Credit Balance
                    var konbiCreditMsg = 'User not found. Please check your card.';
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                }
                else if (transactionInfo.paymentType == 108
                    && transactionInfo.isTokenRetrieved == 1 
                    && transactionInfo.isSufficientFund == 0) {
                    // Show Konbi Credit Balance
                    var konbiCreditMsg = 'Insufficient Balance: $' + transactionInfo.konbiCreditBalance;
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                }
                else if (transactionInfo.paymentType == 108
                    && transactionInfo.isTokenRetrieved == 0) {
                    // Show Konbi Credit Payment API Down
                    var konbiCreditMsg = 'This payment method is currently unavailable.';
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                }
                else if(transactionInfo.paymentType == 109 && transactionInfo.isSufficientFund == 0){
                    // Show Konbi Credit Payment API Down
                    $('#message-processing').text('');
                    var konbiCreditMsg = 'Payment failed: insufficient balance, please top up or choose another payment method.';
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                    
                }

                //$('#change-payment-method').show();
                $('#select-payment').hide();

                background_clear();
                errorSound.play();
                background_failed();
                setTimeout(function () {
                    background_clear();
                }, 3000);
            }

            if (transactionInfo.paymentState == 500) {
                $('#message-processing').addClass('message-error');

                if (transactionInfo.paymentType == 108
                    && transactionInfo.isSufficientFund == 0) {
                    // Show Konbi Credit Balance
                    var konbiCreditMsg = 'Insufficient Balance: $' + transactionInfo.konbiCreditBalance;
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                } else if (transactionInfo.paymentType == 108 && transactionInfo.isTokenRetrieved == 0) {
                    // Show Konbi Credit Payment API Down
                    var konbiCreditMsg = 'This payment method is currently unavailable.';
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                }
                else if(transactionInfo.paymentType == 109){
                    // Show Konbi Credit Payment API Down
                    var konbiCreditMsg = 'Please try again or try another payment method.';
                    $('#konbi-credit-msg').text(konbiCreditMsg);
                    $('#konbi-credit-msg').removeClass();
                    $('#konbi-credit-msg').addClass('message-error message-summary');
                    $('#konbi-credit-msg').show();
                }

                $('#select-payment').hide();

                background_clear();
                background_failed();
                setTimeout(function () {
                    background_clear();
                }, 3000);
                errorSound.play();
            }

            // Wait tap card timeout.
            if (transactionInfo.paymentState == 120) {
                $('#message-processing').addClass('message-error');
                $('#change-payment-method').hide();

                // Error animation and sound
                background_clear();
                background_failed();
                setTimeout(function () {
                    background_clear();
                }, 3000);
                errorSound.play();
            }

            // Wait tap card/look into camera.
            if (transactionInfo.paymentState == 200 || transactionInfo.paymentState == 110) {
                // User remove tray before payment finished.
                if (transactionInfo.customerMessage == 'DO NOT REMOVE ITEMS.') {
                    background_clear();
                    errorSound.play();
                    background_failed();
                    setTimeout(function () {
                        background_clear();
                    }, 3000);
                }

                // Started payment
                if (transactionInfo.customerMessage == 'Please Wait') {
                    $('#message-processing').text('Processing Payment. Please Do Not Move Your Tray');

                    $('#select-payment').hide();
                    $('#show-loading').show();
                    $('#select-mastercardvisa').hide();
                    $('#select-ezlink-netsflashpay').hide();
                    $('#select-qr-dash').hide();
                }
                else {
                    if (transactionInfo.paymentType == 104) {
                        $('#message-processing').text('Mastercard Visa Selected. Please tap your card');

                        $('#select-payment').hide();
                        $('#show-loading').hide();
                        $('#select-mastercardvisa').show();
                        $('#select-ezlink-netsflashpay').hide();

                        background_clear();
                        background_paying();
                    }
                    if (transactionInfo.paymentType == 103) {
                        $('#message-processing').text('Ezlink NetsFlashPay Selected. Please tap your card');

                        $('#select-payment').hide();
                        $('#show-loading').hide();
                        $('#select-mastercardvisa').hide();
                        $('#select-ezlink-netsflashpay').show();

                        background_clear();
                        background_paying();
                    }
                    if (transactionInfo.paymentType == 108) {
                        $('#message-processing').text('Please place your membership card');

                        $('#select-payment').hide();
                        $('#show-loading').hide();
                        $('#select-konbi-credits').show();

                        background_clear();
                        background_paying();
                    }
                    if (transactionInfo.paymentType == 109) {
                        $('#message-processing').text('Please look into the camera device to detect');

                        $('#select-payment').hide();
                        $('#show-loading').hide();
                        $('#select-konbi-credits').show();

                        background_clear();
                        background_paying();
                    }
                    if (transactionInfo.paymentType == 106) {
                        $('#message-processing').text('Please scan QR to pay');

                        $('#select-payment').hide();
                        $('#show-loading').hide();

                        $('#select-mastercardvisa').hide();
                        $('#select-ezlink-netsflashpay').hide();

                        if (transactionInfo.customData && transactionInfo.customData.qr) {
                            console.log('generating qr');

                            $('#qrcode').html('');
                            window.qrc = new QRCode("qrcode");
                            $('#select-qr-dash').show();

                            window.qrc.clear();
                            window.qrc.makeCode(transactionInfo.customData.qr);
                        }

                        background_clear();
                        background_paying();
                    }
                }

                enablePaymentSound.play();
            } else {
                $('#qrcode').html('');
            }

            // Cancel transaction
            if (transactionInfo.customerMessage == "Cancelling transaction...") {
                background_clear();
            }

            // Active Payment.
            if (transactionInfo.paymentState == 105) {
                $('#select-payment').hide();
                $('#show-loading').show();
                $('#select-mastercardvisa').hide();
                $('#select-ezlink-netsflashpay').hide();
                $('#select-qr-dash').hide();
                $('#select-konbi-credits').hide();

                enablePaymentSound.play();
            }
            
            //Failed to activate payment
            if (transactionInfo.paymentState == 115) {
                $('#message-processing').text('Failed to activate payment device');
                $('#message-processing').addClass('message-failed');

                background_clear();
                errorSound.play();
                background_failed();
                setTimeout(function () {
                    background_clear();
                }, 3000);
            }

            // Show plate after scan.
            $('.row-items').remove();

            if (transactionInfo.menuItems) {

                for (var i = 0; i < transactionInfo.menuItems.length; i++) {

                    var itemOrder = `<div class="row row-items">
                                        <div class="col-1 col-sm-1">
                                            ${i + 1}
                                        </div>
                                        <div class="col-8 col-sm-8" style="text-align: left;">
                                            ${transactionInfo.menuItems[i].productName}
                                        </div>
                                        <div class="col-3 col-sm-2">
                                            $${transactionInfo.menuItems[i].price.toFixed(2)}
                                        </div>
                                    </div>`;

                    $('#rounded-list').append(itemOrder);
                }

                if (transactionInfo.taxAmount > 0) {
                    var ItemAmountExclTax = `<div class="row row-items">
                            <div class="col-1 col-sm-1">
                            
                            </div>
                            <div class="col-8 col-sm-8" style="text-align: right;">
                            Amount Excl ${transactionInfo.taxName}:
                            </div>
                            <div class="col-3 col-sm-2">
                                $${transactionInfo.amountBeforeTax.toFixed(2)}
                            </div>
                        </div>`;
                    $('#rounded-list').append(ItemAmountExclTax);
                    var ItemTax = `<div class="row row-items">
                            <div class="col-1 col-sm-1">
                            
                            </div>
                            <div class="col-8 col-sm-8" style="text-align: right;">
                            ${transactionInfo.taxName}:
                            </div>
                            <div class="col-3 col-sm-2">
                                $${transactionInfo.taxAmount.toFixed(2)}
                            </div>
                        </div>`;
                    $('#rounded-list').append(ItemTax);
                }
                if (transactionInfo.discountAmount > 0) {
                    var ItemDiscount = `<div class="row row-items">
                        <div class="col-1 col-sm-1">
                        
                        </div>
                        <div class="col-8 col-sm-8" style="text-align: right;">
                        Discount:
                        </div>
                        <div class="col-3 col-sm-2">
                            $${transactionInfo.discountAmount.toFixed(2)}
                        </div>
                    </div>`;
                    $('#rounded-list').append(ItemDiscount);
                }
            }
            // Set total price.
            $('#price').text('$' + transactionInfo.total.toFixed(2));
            $('#transaction').val('');
            setTimeout(function () {
                $('#transaction').val(JSON.stringify(transactionInfo));
            }, 100);

            fitMessage();
        });

        connection.on('updateSessionInfo', function (sessionInfo) {
            console.log('Update session on UI');
            console.log(sessionInfo);
            if (sessionInfo)
                $('#text-session-info').html(sessionInfo.name + ' ' + sessionInfo.fromHrs + '-' + sessionInfo.toHrs);
            else
                $('#text-session-info').html("<span class='text-danger'>No appropriate session</span>");
        });

        connection.on('checkServiceStatus', function (checkServiceStatus) {
            console.log('updated device status on UI');
            checkServiceStatus.forEach(element => {
                switch (element.type) {
                    //1. Show Server connection status
                    case "MagicPlateServer.Service":
                        if (element.status == true) {
                            $('#connection-status>#server')
                                .removeClass('text-danger')
                                .addClass('fa-server text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#server')
                                .removeClass('text-success')
                                .addClass('text-danger fa-desktop')
                                .attr("title", element.message);
                        }
                        break;
                    //2. Show RabbitMQ connection status
                    case "RabbitMqServer.Service":
                        if (element.status) {
                            $('#connection-status>#rabbitmq')
                                .removeClass('text-danger')
                                .addClass('fa-car-battery text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#rabbitmq')
                                .removeClass('text-success')
                                .addClass('text-danger fa-car-battery')
                                .attr("title", element.message);
                        }
                        break;
                    //3. Show RFID connection status
                    case "TagReader.Service":
                        if (element.status) {
                            $('#connection-status>#rfid')
                                .removeClass('text-danger')
                                .addClass('fa-broadcast-tower text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#rfid')
                                .removeClass('text-success')
                                .addClass('text-danger fa-broadcast-tower')
                                .attr("title", element.message);
                        }
                        break;
                    //4. Show LightAlarm connection status
                    case "LightAlarm.Service":
                        if (element.status) {
                            $('#connection-status>#lightalarm')
                                .removeClass('text-danger')
                                .addClass('fa-lightbulb text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#lightalarm')
                                .removeClass('text-success')
                                .addClass('text-danger fa-lightbulb')
                                .attr("title", element.message);
                        }
                        break;
                    //5. Show Camera connection status
                    case "Camera.Service":
                        if (element.status) {
                            $('#connection-status>#camera')
                                .removeClass('text-danger')
                                .addClass('fa-camera text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#camera')
                                .removeClass('text-success')
                                .addClass('text-danger fa-camera')
                                .attr("title", element.message);
                        }
                        break;
                    //6. Show IUC connection status
                    case "PaymentController.Service":
                        if (element.status) {
                            $('#connection-status>#iuc')
                                .removeClass('text-danger')
                                .addClass('fa-money-check-alt text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#iuc')
                                .removeClass('text-success')
                                .addClass('text-danger fa-money-check-alt')
                                .attr("title", element.message);
                        }
                        break;
                    //7. Show Konbi Credits Card Reader connection status
                    case "KonbiCreditsPaymentController.Service":
                        if (element.status) {
                            $('#connection-status>#konbiCredits')
                                .removeClass('text-danger')
                                .addClass('fa-id-card text-success')
                                .attr("title", element.message);
                        }
                        else {
                            $('#connection-status>#konbiCredits')
                                .removeClass('text-success')
                                .addClass('text-danger fa-id-card')
                                .attr("title", element.message);
                        }
                        break;
                    default:
                        //nothing
                        break;
                }

            });
        });

        connection.on('updatePaymentMethods', function (acceptedPayments) {
            console.log('accepted payment methods:');
            console.log(acceptedPayments);
            var paymentBtnGroup = $('.payment-btn-group');
            paymentBtnGroup.find('.col:gt(0)').remove();

            var btnTemplate = paymentBtnGroup.find('.payment-btn-template');

            for (var paymentMethod in acceptedPayments) {
                var addingBtn = btnTemplate.clone().removeClass('d-none payment-btn-template');
                addingBtn.data('method', paymentMethod);
                var caption = acceptedPayments[paymentMethod];
                var imgUrl = "";
                if (caption.toLowerCase() == "ezlink") {
                    caption = "EZLink Flashpay";
                    imgUrl = '../images/logo/Ezlink_NetsFlashPay.jpg';
                } else if (caption.toLowerCase() == "contactless") {
                    caption = "Visa Mastercard";
                    imgUrl = '../images/logo/Mastercard_Visa.jpg';
                } else if (caption.toLowerCase() == "dash qr") {
                    caption = "Dash QR";
                    imgUrl = '../images/logo/qr_dash.jpg';
                } else if (caption.toLowerCase() == "konbi credits") {
                    caption = "Baxter Credits";
                    imgUrl = '../images/logo/baxter_logo.jpg';
                }
                else if (caption.toLowerCase() == "facial_recognition") {
                    caption = "Facial Recog";
                    imgUrl = '../images/logo/facialrecog.png';
                }
                addingBtn.find('.title-logo-payment').html(caption);
                addingBtn.find('.image-logo-payment').attr('src', imgUrl);
                paymentBtnGroup.append(addingBtn);
            }

        });

        connection.on('updatePosModeStatus', ({
            isPosModeOn
        }) => {
            handlePOSModeChange(isPosModeOn);
        });

        connection.on('showCustomerCountDown', function (countDownInfo) {
            setCountDown(countDownInfo.countTime, countDownInfo.isOff);
        });

        function fitMessage() {
            $('#customer-message').textfill({
                minFontPixels: 16,
                maxFontPixels: 24,
                explicitHeight: 270,
                explicitWidth: 420,
                debug: true
            });
        }
    }

    // Connect to the server
    abp.signalr.connect = function () {
        var url = abp.signalr.url || (abp.appPath + 'signalr-rfidtable');

        // Start the connection.
        startConnection(url, configureConnection)
            .then(function (connection) {
                $('.cycle-status').css({
                    "color": "green"
                });
                var isConnectedToGroup = false;
                connection.invoke('JoinGroup', "CustomerUI").then(function (result) {
                    console.log('join group result:' + result);
                    isConnectedToGroup = result;


                });


                connection.invoke('GetSessionInfo').then(function (sessionInfo) {
                    $('#text-session-info').html(sessionInfo.name + ' ' + sessionInfo.fromHrs + '-' + sessionInfo.toHrs);
                }).catch(() => { });

                connection.invoke('GetPOSModeStatus').then((isPosModeOn) => {
                    handlePOSModeChange(isPosModeOn);
                }).catch(() => { });
            })
            .catch(() => {
                $('.cycle-status').css({
                    "color": "red"
                });
            });
    };

    // Starts a connection with transport fallback - if the connection cannot be started using
    // the webSockets transport the function will fallback to the serverSentEvents transport and
    // if this does not work it will try longPolling. If the connection cannot be started using
    // any of the available transports the function will return a rejected Promise.
    function startConnection(url, configureConnection) {
        if (abp.signalr.remoteServiceBaseUrl) {
            url = abp.signalr.remoteServiceBaseUrl + url;
        }

        // Add query string: https://github.com/aspnet/SignalR/issues/680
        if (abp.signalr.qs) {
            url += (url.indexOf('?') == -1 ? '?' : '&') + abp.signalr.qs;
        }

        return function start(transport) {
            updateConnectStateIcon('connecting');
            console.log('Starting connection using ' + signalR.HttpTransportType[transport] + ' transport');
            var connection = new signalR.HubConnectionBuilder()
                .withUrl(url, transport)
                .build();
            if (configureConnection && typeof configureConnection === 'function') {
                configureConnection(connection);
            }

            return connection.start() // force to use only Websocket protocol
                .then(function () {
                    updateConnectStateIcon('connected');
                    return connection;
                })
                .catch(function (error) {
                    updateConnectStateIcon('disconnected');
                    console.log('Cannot start the connection using ' + signalR.HttpTransportType[transport] + ' transport. ' + error.message);
                    if (transport !== signalR.HttpTransportType.LongPolling) {
                        return start(signalR.HttpTransportType.WebSockets);
                    }

                    return Promise.reject(error);
                });
        }(signalR.HttpTransportType.WebSockets);
    }

    function cancelPayment() {
        console.log('CancelTransaction');
        if (abp.signalr.transaction.paymentState == 110 || 1 == 1) // payment device is activated.
        {
            abp.signalr.hubs.common.invoke("CancelTransaction").then(function (cancelResult) {
                console.log(cancelResult);
            });
        } else {
            console.log("admin key  pressed. but payment device is not acivated. No need to cancel transaction.");
        }
    }

    abp.signalr.startConnection = startConnection;

    if (abp.signalr.autoConnect === undefined) {
        abp.signalr.autoConnect = true;
    }

    if (abp.signalr.autoConnect) {
        abp.signalr.connect();
    }

    var currentCountDown;

    function setCountDown(countTime, isOff) {
        if (isOff) {
            clearInterval(currentCountDown);
            $('#count-down').text("");
            $('#show-count-down').addClass('d-none');
        } else {
            clearInterval(currentCountDown);
            currentCountDown = setInterval(function () {
                if (countTime >= 0) {
                    $('#count-down').text(countTime);
                    $('#show-count-down').removeClass('d-none');
                }
                //add buffer 3 seconds before cancelling transaction.
                if (countTime < -2) {
                    clearInterval(currentCountDown);
                    $('#count-down').text("");
                    $('#show-count-down').addClass('d-none');
                    //TrungPQ: Add new flow of Cotf.
                    abp.signalr.hubs.common.invoke("CancelTransaction").then(function (cancelResult) {
                        console.log(cancelResult);
                    });
                }
                countTime--;
            }, 1000);
        }
    }

    function updateConnectStateIcon(connState) {
        console.log(connState);
        switch (connState) {
            case "connected":
                $('#connection-status>#slaveServer').removeClass('fa-spin fa-spinner text-danger').addClass('fa-desktop text-success');
                break;
            case "connecting":
                $('#connection-status>#slaveServer').removeClass('fa-wifi text-success').addClass('fa-spinner fa-spin text-danger');
                break;
            case "disconnected":
                $('#connection-status>#slaveServer').removeClass('fa-spinner fa-wifi text-success').addClass('fa-wifi-slash text-danger');
                break;
        }
    }
})();