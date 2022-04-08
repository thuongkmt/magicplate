var konbini = konbini || {};
(function () {

  /* Swagger */

  konbini.swagger = konbini.swagger || {};

  konbini.swagger.addAuthToken = function () {
    var authToken = konbini.auth.getToken();
    if (!authToken) {
      return false;
    }

    var cookieAuth = new SwaggerClient.ApiKeyAuthorization(konbini.auth.tokenHeaderName, 'Bearer ' + authToken, 'header');
    swaggerUi.api.clientAuthorizations.add('bearerAuth', cookieAuth);
    return true;
  }

  konbini.swagger.addCsrfToken = function () {
    var csrfToken = konbini.security.antiForgery.getToken();
    if (!csrfToken) {
      return false;
    }
    var csrfCookieAuth = new SwaggerClient.ApiKeyAuthorization(konbini.security.antiForgery.tokenHeaderName, csrfToken, 'header');
    swaggerUi.api.clientAuthorizations.add(konbini.security.antiForgery.tokenHeaderName, csrfCookieAuth);
    return true;
  }

  function loginUserInternal(tenantId, callback) {
    var usernameOrEmailAddress = document.getElementById('userName').value;
    if (!usernameOrEmailAddress) {
      //alert('Username is required, please try with a valid value !');
      document.getElementById('err-msg').innerText = "Username is required, please try with a valid value !";
      return false;
    }

    var password = document.getElementById('password').value;
    if (!password) {
      //alert('Password is required, please try with a valid value !');
      document.getElementById('err-msg').innerText = "Password is required, please try with a valid value !!";
      return false;
    }

    var xhr = new XMLHttpRequest();

    xhr.onreadystatechange = function () {
      if (xhr.readyState === XMLHttpRequest.DONE) {
        if (xhr.status === 200) {
          var responseJSON = JSON.parse(xhr.responseText);
          console.log(responseJSON);
          var result = responseJSON;
          var expireDate = new Date(Date.now() + (99999999999999 * 1000));
          konbini.auth.setToken(result.access_token, expireDate);
          callback();
        } else {
          document.getElementById('err-msg').innerText = "Login failed !";
        }
      }
    };

    xhr.open('GET', '/token?username=' + usernameOrEmailAddress + '&password=' + password, true);
    xhr.send();
  };

  function getHardwareStatus(callback) {

    var xhr = new XMLHttpRequest();

    xhr.onreadystatechange = function () {
      if (xhr.readyState === XMLHttpRequest.DONE) {
        if (xhr.status === 200) {
          var responseJSON = xhr.responseText;//JSON.parse(xhr.responseText);
          callback(responseJSON);
        }
      }
    };

    xhr.open('GET', '/api/hardware/status', true);
    xhr.send();
  };

  konbini.swagger.hwstatus = function (callback) {
    getHardwareStatus(callback);
  };

  konbini.swagger.login = function (callback) {
    loginUserInternal(null, callback); // Login for host
  };

  konbini.swagger.logout = function (callback) {
    var authToken = konbini.auth.getToken();
    if (!authToken) {
      return true;
    }
    konbini.auth.clearToken();
    callback && callback();
  }

  konbini.swagger.closeAuthDialog = function () {
    if (document.getElementById('konbini-auth-dialog')) {
      document.getElementsByClassName("swagger-ui")[1].removeChild(document.getElementById('konbini-auth-dialog'));
    }
  }

  konbini.swagger.openAuthDialog = function (loginCallback) {
    konbini.swagger.closeAuthDialog();

    var konbiniAuthDialog = document.createElement('div');
    konbiniAuthDialog.className = 'dialog-ux';
    konbiniAuthDialog.id = 'konbini-auth-dialog';

    document.getElementsByClassName("swagger-ui")[1].appendChild(konbiniAuthDialog);

    // -- backdrop-ux
    var backdropUx = document.createElement('div');
    backdropUx.className = 'backdrop-ux';
    konbiniAuthDialog.appendChild(backdropUx);

    // -- modal-ux
    var modalUx = document.createElement('div');
    modalUx.className = 'modal-ux';
    konbiniAuthDialog.appendChild(modalUx);

    // -- -- modal-dialog-ux
    var modalDialogUx = document.createElement('div');
    modalDialogUx.className = 'modal-dialog-ux';
    modalUx.appendChild(modalDialogUx);

    // -- -- -- modal-ux-inner
    var modalUxInner = document.createElement('div');
    modalUxInner.className = 'modal-ux-inner';
    modalDialogUx.appendChild(modalUxInner);

    // -- -- -- -- modal-ux-header
    var modalUxHeader = document.createElement('div');
    modalUxHeader.className = 'modal-ux-header';
    modalUxInner.appendChild(modalUxHeader);

    var modalHeader = document.createElement('h3');
    modalHeader.innerText = 'Authorize';
    modalUxHeader.appendChild(modalHeader);

    // -- -- -- -- modal-ux-content
    var modalUxContent = document.createElement('div');
    modalUxContent.className = 'modal-ux-content';
    modalUxInner.appendChild(modalUxContent);

    modalUxContent.onkeydown = function (e) {
      if (e.keyCode === 13) {
        //try to login when user presses enter on authorize modal
        konbini.swagger.login(loginCallback);
      }
    };

    //Inputs
    //createInput(modalUxContent, 'tenancyName', 'Tenancy Name (Leave empty for Host)');
    createInput(modalUxContent, 'userName', 'Username');
    createInput(modalUxContent, 'password', 'Password', 'password');

    // Error message
    var wrapper = document.createElement('div');
    wrapper.className = 'wrapper';
    modalUxContent.appendChild(wrapper);

    var label = document.createElement('label');
    label.innerText = "";
    label.id = "err-msg";
    label.style = "color: red;";
    wrapper.appendChild(label);

    //Buttons
    var authBtnWrapper = document.createElement('div');
    authBtnWrapper.className = 'auth-btn-wrapper';
    modalUxContent.appendChild(authBtnWrapper);

    //Close button
    var closeButton = document.createElement('button');
    closeButton.className = 'btn modal-btn auth btn-done button';
    closeButton.innerText = 'Close';
    closeButton.style.marginRight = '5px';
    closeButton.onclick = konbini.swagger.closeAuthDialog;
    authBtnWrapper.appendChild(closeButton);

    //Authorize button
    var authorizeButton = document.createElement('button');
    authorizeButton.className = 'btn modal-btn auth authorize button';
    authorizeButton.innerText = 'Login';
    authorizeButton.onclick = function () {
      konbini.swagger.login(loginCallback);
    };
    authBtnWrapper.appendChild(authorizeButton);
  }

  function createInput(container, id, title, type) {
    var wrapper = document.createElement('div');
    wrapper.className = 'wrapper';
    container.appendChild(wrapper);

    var label = document.createElement('label');
    label.innerText = title;
    wrapper.appendChild(label);

    var section = document.createElement('section');
    section.className = 'block-tablet col-10-tablet block-desktop col-10-desktop';
    wrapper.appendChild(section);

    var input = document.createElement('input');
    input.id = id;
    input.type = type ? type : 'text';
    input.style.width = '100%';

    section.appendChild(input);
  }



})();