import { type ReactNode, useState } from 'react'
import * as Dialog from '@radix-ui/react-dialog'
import { useToast } from './Toast'

type AuthModalProps = {
  trigger: ReactNode
  onAuthenticated?: () => void
}

function AuthModal({ trigger, onAuthenticated }: AuthModalProps) {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const { showToast } = useToast()

  const handleAuthenticated = () => {
    if (mode === 'login') {
      onAuthenticated?.()
      showToast('Đăng nhập thành công', 'Chào mừng bạn trở lại với Andev EShop!', 'success')
    } else {
      // Test: show error toast for register
      showToast('Đăng ký thất bại', 'Email đã được sử dụng. Vui lòng thử email khác.', 'error')
    }
  }

  return (
    <Dialog.Root>
      <Dialog.Trigger asChild>{trigger}</Dialog.Trigger>
      <Dialog.Portal>
        <Dialog.Overlay className="radix-overlay" />
        <Dialog.Content className="radix-modal" data-mode={mode}>
          <Dialog.Close className="modal-close-btn icon-button" aria-label="Đóng">
            ✕
          </Dialog.Close>
          <div className="modal-layout">
            <div className="modal-left">
              <form className={`modal-form ${mode}`}>
                <div className="form-grid">
                  {mode === 'register' && (
                    <>
                      <label>
                        Tên (tuỳ chọn)
                        <input type="text" placeholder="Tên hiển thị" />
                      </label>
                      <label>
                        Số điện thoại (tuỳ chọn)
                        <input type="tel" placeholder="Số điện thoại" />
                      </label>
                    </>
                  )}
                  <label>
                    Email
                    <input type="email" placeholder="you@example.com" />
                  </label>
                  <label>
                    Mật khẩu
                    <input type="password" placeholder="••••••••" />
                  </label>
                  {mode === 'register' && (
                    <>
                      <label>
                        Xác nhận mật khẩu
                        <input type="password" placeholder="••••••••" />
                      </label>
                      <label>
                        Địa chỉ (tuỳ chọn)
                        <input type="text" placeholder="Địa chỉ giao hàng" />
                      </label>
                    </>
                  )}
                </div>

                {mode === 'login' && (
                  <button className="link-button" type="button">
                    Quên mật khẩu?
                  </button>
                )}

                <Dialog.Close asChild>
                  <button className="btn btn-primary" type="button" onClick={handleAuthenticated}>
                    {mode === 'login' ? 'Đăng nhập' : 'Tạo tài khoản'}
                  </button>
                </Dialog.Close>
              </form>
            </div>

            <div className="modal-right">
              <div className="modal-header">
                <div>
                  <Dialog.Title className="modal-title">
                    {mode === 'login' ? 'Đăng nhập' : 'Đăng ký'}
                  </Dialog.Title>
                  <Dialog.Description className="modal-subtitle">
                    {mode === 'login'
                      ? 'Chào mừng trở lại với Andev EShop'
                      : 'Tạo tài khoản để lưu sản phẩm yêu thích.'}
                  </Dialog.Description>
                </div>
              </div>

              <div className="modal-switch" data-mode={mode}>
                <span className="switch-indicator" />
                <button
                  type="button"
                  className={mode === 'login' ? 'active' : ''}
                  onClick={() => setMode('login')}
                >
                  Đăng nhập
                </button>
                <button
                  type="button"
                  className={mode === 'register' ? 'active' : ''}
                  onClick={() => setMode('register')}
                >
                  Đăng ký
                </button>
              </div>

              <div className="modal-divider">
                <span>Hoặc</span>
              </div>

              <div className="social-row">
                <button className="btn btn-ghost social-google" type="button">
                  Google
                </button>
                <button className="btn btn-ghost social-facebook" type="button">
                  Facebook
                </button>
              </div>
            </div>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

export default AuthModal
