
$(document).ready(function () {
    
    initializeValidation();

    // Real-time validation on input
    $('#signupForm input').on('input blur', function () {
        validateField($(this));
    });

    // Password strength indicator
    $('#Password').on('input', function () {
        updatePasswordStrength($(this).val());
    });

    // Confirm password real-time check
    $('#ConfirmPassword').on('input', function () {
        validateConfirmPassword();
    });
});

function initializeValidation() {
    // Add custom validation methods
    $.validator.addMethod('maxLength', function (value, element, param) {
        return value.length <= param;
    }, 'Please enter no more than {0} characters.');

    $.validator.addMethod('strongPassword', function (value, element) {
        return validatePasswordStrength(value);
    }, 'Password must contain at least 8 characters, one uppercase, one lowercase, one number, and one special character.');

    // Main validation rules
    $('#signupForm').validate({
        rules: {
            Username: {
                required: true,
                minlength: 3,
                maxlength: 20,
                alphanumeric: true
            },
            Email: {
                required: true,
                email: true,
                maxlength: 100
            },
            Password: {
                required: true,
                minlength: 8,
                strongPassword: true
            },
            ConfirmPassword: {
                required: true,
                equalTo: '#Password'
            }
        },
        messages: {
            Username: {
                required: 'Username is required.',
                minlength: 'Username must be at least 3 characters long.',
                maxlength: 'Username cannot exceed 20 characters.',
                alphanumeric: 'Username can only contain letters and numbers.'
            },
            Email: {
                required: 'Email address is required.',
                email: 'Please enter a valid email address.',
                maxlength: 'Email cannot exceed 100 characters.'
            },
            Password: {
                required: 'Password is required.',
                minlength: 'Password must be at least 8 characters long.'
            },
            ConfirmPassword: {
                required: 'Please confirm your password.',
                equalTo: 'Passwords do not match.'
            }
        },
        errorClass: 'field-validation-error',
        validClass: 'field-validation-valid',
        errorElement: 'span',
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        submitHandler: function (form) {
            // Custom submit handling
            if ($(form).valid()) {
                // Show loading state
                $('.btn-signup').prop('disabled', true).text('Creating Account...');

                // Submit the form
                form.submit();
            }
        }
    });

    // Add custom validation for alphanumeric
    $.validator.addMethod('alphanumeric', function (value, element) {
        return this.optional(element) || /^[a-zA-Z0-9_]+$/.test(value);
    }, 'Username can only contain letters, numbers, and underscores.');
}

function validateField(field) {
    var fieldId = field.attr('id');
    var value = field.val();
    var isValid = true;
    var errorMessage = '';

    switch (fieldId) {
        case 'Username':
            isValid = validateUsername(value);
            errorMessage = 'Username must be 3-20 characters (letters, numbers, underscores).';
            break;
        case 'Email':
            isValid = validateEmail(value);
            errorMessage = 'Please enter a valid email address.';
            break;
        case 'Password':
            isValid = validatePasswordStrength(value);
            errorMessage = 'Password must contain at least 8 characters, one uppercase, one lowercase, one number, and one special character.';
            break;
        case 'ConfirmPassword':
            isValid = validateConfirmPassword();
            errorMessage = 'Passwords do not match.';
            break;
        default:
            return;
    }

    // Update visual feedback
    var errorSpan = field.siblings('.field-validation-error');
    var formGroup = field.closest('.form-group');

    if (!isValid && value.length > 0) {
        errorSpan.text(errorMessage).show();
        formGroup.addClass('has-error').removeClass('has-success');
        field.addClass('input-validation-error').removeClass('input-validation-valid');
    } else if (value.length > 0) {
        errorSpan.text('').hide();
        formGroup.removeClass('has-error').addClass('has-success');
        field.removeClass('input-validation-error').addClass('input-validation-valid');
    } else {
        errorSpan.text('').hide();
        formGroup.removeClass('has-error has-success');
        field.removeClass('input-validation-error input-validation-valid');
    }
}

function validateUsername(username) {
    if (username.length < 3 || username.length > 20) {
        return false;
    }
    // Allow letters, numbers, and underscores
    return /^[a-zA-Z0-9_]+$/.test(username);
}

function validateEmail(email) {
    var emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(email);
}

function validatePasswordStrength(password) {
    if (password.length < 8) {
        return false;
    }

    var hasUppercase = /[A-Z]/.test(password);
    var hasLowercase = /[a-z]/.test(password);
    var hasNumber = /[0-9]/.test(password);
    var hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);

    return hasUppercase && hasLowercase && hasNumber && hasSpecial;
}

function validateConfirmPassword() {
    var password = $('#Password').val();
    var confirmPassword = $('#ConfirmPassword').val();

    var confirmField = $('#ConfirmPassword');
    var errorSpan = confirmField.siblings('.field-validation-error');
    var formGroup = confirmField.closest('.form-group');

    if (confirmPassword.length > 0 && password !== confirmPassword) {
        errorSpan.text('Passwords do not match.').show();
        formGroup.addClass('has-error').removeClass('has-success');
        confirmField.addClass('input-validation-error').removeClass('input-validation-valid');
        return false;
    } else if (confirmPassword.length > 0) {
        errorSpan.text('').hide();
        formGroup.removeClass('has-error').addClass('has-success');
        confirmField.removeClass('input-validation-error').addClass('input-validation-valid');
        return true;
    } else {
        errorSpan.text('').hide();
        formGroup.removeClass('has-error has-success');
        confirmField.removeClass('input-validation-error input-validation-valid');
        return false;
    }
}

function updatePasswordStrength(password) {
    var strength = 0;
    var strengthText = '';
    var strengthClass = '';

    if (password.length === 0) {
        $('#password-strength').remove();
        return;
    }

    // Check password strength criteria
    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

    // Determine strength level
    if (strength <= 2) {
        strengthText = 'Weak';
        strengthClass = 'weak';
    } else if (strength <= 3) {
        strengthText = 'Medium';
        strengthClass = 'medium';
    } else {
        strengthText = 'Strong';
        strengthClass = 'strong';
    }

    // Add or update strength indicator
    var strengthDiv = $('#password-strength');
    if (strengthDiv.length === 0) {
        strengthDiv = $('<div id="password-strength" class="password-strength"></div>');
        $('#Password').after(strengthDiv);
    }

    strengthDiv.html('Password Strength: <span class="' + strengthClass + '">' + strengthText + '</span>')
        .show();
}