var selectedCompany = 0;
var selectedRole = 0;
var selectedCompanyText = "";
var selectedRoleText = "";
var roleDropdownListEnabled = false;
var companyDropdownListEnabled = false;


function checkRoleDropdownListEnabled() {
    var roleDisplay = $("#user_rolenumber").css("display");

    if (roleDisplay != "none" && roleDisplay != undefined) {
        roleDropdownListEnabled = true;
    }
    else
    {
        roleDropdownListEnabled = false;
    }
}

function checkCompanyDropdownListEnabled() {
    var companyDisplay = $("#user_companyId").css("display");

    if (companyDisplay != "none" && companyDisplay != undefined) {
        companyDropdownListEnabled = true;
    }
    else
    {
        companyDropdownListEnabled = false;
    }
}

function initilizeRole()
{
    selectedRole = $('#user_rolenumber Option:Selected').val();
    selectedRoleText = $('#user_rolenumber Option:Selected').text();
    selectedCompany = $('#user_companyId Option:Selected').val();
    selectedCompanyText = $('#user_companyId Option:Selected').text();

    if (selectedRole == 4) {
        $('#user_companyId').css("display", "black");
        $('#label_companyId').show();
    }
    else
    {
        $('#user_companyId').css("display", "none");
        $('#label_companyId').hide();
    }
}

$('#user_rolenumber').change(function(){
    selectedRoleText = $('#user_rolenumber Option:Selected').text();
    selectedRole = $('#user_rolenumber Option:Selected').val();

    if (selectedRoleText != 'Company')
    {
        companyDropdownListEnabled = false;
        $('#user_companyId').val("0");
        selectedCompany = $('#user_companyId Option:Selected').val();
        $('#user_companyId Option:Selected').text = "";
        selectedCompanyText = $('#user_companyId Option:Selected').text();
        $('#user_companyId').css("display", "none");
        $('#label_companyId').hide();
    }
    else
    {
        companyDropdownListEnabled = true;
        $('#user_companyId').css("display", "block");
        $('#label_companyId').show();
    }
});

$('#user_companyId').change(function(){
    selectedCompany = $('#user_companyId Option:Selected').val();
    selectedCompanyText = $('#user_companyId Option:Selected').text();
    companyDropdownListEnabled = true;
});

$('#editUserInfo').click(function(){
    if (roleDropdownListEnabled && selectedRole == 0)
    {
        showMessage("User Edit Page", "Please select a role.");
        return false;
    }
    if (companyDropdownListEnabled && selectedCompany == 0)
    {
        showMessage("User Edit Page", "Please select a company.");
        return false;
    }

    return true;
});

$(document).ready(function(){
    initilizeRole();
});