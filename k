[1mdiff --git a/PDFUploadWebApplication/Controllers/AuthController.cs b/PDFUploadWebApplication/Controllers/AuthController.cs[m
[1mindex d89ccef..f383a7f 100644[m
[1m--- a/PDFUploadWebApplication/Controllers/AuthController.cs[m
[1m+++ b/PDFUploadWebApplication/Controllers/AuthController.cs[m
[36m@@ -83,7 +83,11 @@[m [mpublic class AuthController : Controller[m
             return View(model);[m
         }[m
 [m
[31m-        // Check if the username already exists[m
[32m+[m[32m        // ✅ Convert username and email to lowercase for consistency[m
[32m+[m[32m        model.Username = model.Username.ToLower();[m
[32m+[m[32m        model.Email = model.Email.ToLower();[m
[32m+[m
[32m+[m[32m        // Check if the username already exists (case-insensitive)[m
         var existingUser = await _userRepository.GetByUsernameAsync(model.Username);[m
         if (existingUser != null)[m
         {[m
[36m@@ -96,8 +100,8 @@[m [mpublic class AuthController : Controller[m
 [m
         var newUser = new User[m
         {[m
[31m-            Username = model.Username,[m
[31m-            Email = model.Email,[m
[32m+[m[32m            Username = model.Username,  // ✅ Ensure stored username is lowercase[m
[32m+[m[32m            Email = model.Email,        // ✅ Ensure stored email is lowercase[m
             PasswordHash = hash,[m
             PasswordSalt = salt[m
         };[m
[36m@@ -109,8 +113,6 @@[m [mpublic class AuthController : Controller[m
 [m
 [m
 [m
[31m-[m
[31m-[m
     private string GenerateJwtToken(User user)[m
     {[m
         var jwtSettings = _configuration.GetSection("Jwt");[m
[1mdiff --git a/PDFUploadWebApplication/Views/Auth/Register.cshtml b/PDFUploadWebApplication/Views/Auth/Register.cshtml[m
[1mindex 32c2856..3c68ad9 100644[m
[1m--- a/PDFUploadWebApplication/Views/Auth/Register.cshtml[m
[1m+++ b/PDFUploadWebApplication/Views/Auth/Register.cshtml[m
[36m@@ -67,7 +67,9 @@[m
         </div>[m
     </div>[m
 </div>[m
[31m-[m
[32m+[m[32m<div id="validationPopup" class="alert alert-danger d-none position-fixed top-0 start-50 translate-middle-x mt-2" style="z-index: 1050;">[m
[32m+[m[32m    <p id="validationMessage"></p>[m
[32m+[m[32m</div>[m
 <script>[m
     document.getElementById("registerForm").addEventListener("submit", async function(event) {[m
         event.preventDefault();[m
[36m@@ -92,11 +94,50 @@[m
         } else {[m
             const result = await response.json();[m
             console.error(result);[m
[31m-            alert("Registration failed: " + (result.message || "Unknown error"));[m
[32m+[m
[32m+[m[32m            // ✅ Show validation error in a popup for 5 seconds[m
[32m+[m[32m            const popup = document.getElementById("validationPopup");[m
[32m+[m[32m            const message = document.getElementById("validationMessage");[m
[32m+[m
[32m+[m[32m            message.innerText = result.message || "Registration failed due to unknown error.";[m
[32m+[m[32m            popup.classList.remove("d-none");[m
[32m+[m
[32m+[m[32m            setTimeout(() => {[m
[32m+[m[32m                popup.classList.add("d-none"); // ✅ Hide after 5 seconds[m
[32m+[m[32m            }, 5000);[m
         }[m
     });[m
 </script>[m
 [m
[32m+[m[32m@* <script>[m
[32m+[m[32m    document.getElementById("registerForm").addEventListener("submit", async function(event) {[m
[32m+[m[32m        event.preventDefault();[m
[32m+[m
[32m+[m[32m        const data = {[m
[32m+[m[32m            username: document.getElementById("username").value,[m
[32m+[m[32m            email: document.getElementById("email").value,[m
[32m+[m[32m            password: document.getElementById("password").value,[m
[32m+[m[32m            confirmPassword: document.getElementById("confirmPassword").value[m
[32m+[m[32m        };[m
[32m+[m
[32m+[m[32m        const response = await fetch('/Auth/register', {[m
[32m+[m[32m            method: 'POST',[m
[32m+[m[32m            headers: {[m
[32m+[m[32m                'Content-Type': 'application/json'[m
[32m+[m[32m            },[m
[32m+[m[32m            body: JSON.stringify(data)[m
[32m+[m[32m        });[m
[32m+[m
[32m+[m[32m        if (response.ok) {[m
[32m+[m[32m            window.location.href = "/Auth/login"; // ✅ Redirects to login page[m
[32m+[m[32m        } else {[m
[32m+[m[32m            const result = await response.json();[m
[32m+[m[32m            console.error(result);[m
[32m+[m[32m            alert("Registration failed: " + (result.message || "Unknown error"));[m
[32m+[m[32m        }[m
[32m+[m[32m    });[m
[32m+[m[32m</script>[m
[32m+[m[32m *@[m
 @section Scripts {[m
     <partial name="_ValidationScriptsPartial" />[m
 }[m
