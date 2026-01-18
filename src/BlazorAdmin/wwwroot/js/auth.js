// BlazorAdmin authentication helper
window.blazorAuth = {
    login: async function (username, password) {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password }),
                credentials: 'same-origin' // Important: include cookies
            });

            const data = await response.json();
            
            if (response.ok) {
                return { success: true, message: null };
            } else {
                return { success: false, message: data.message || 'Đăng nhập thất bại' };
            }
        } catch (error) {
            return { success: false, message: error.message };
        }
    },
    
    logout: async function () {
        try {
            await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'same-origin'
            });
            return true;
        } catch {
            return false;
        }
    }
};
