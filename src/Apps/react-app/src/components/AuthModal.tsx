import { type ReactNode, useState } from 'react'
import * as Dialog from '@radix-ui/react-dialog'
import axios from 'axios'
import { useToast } from './Toast'
import { useAuth } from '../hooks/useAuth'
import { getOrCreateGuestId } from '../config/api'

type AuthModalProps = {
  trigger: ReactNode
}

function AuthModal({ trigger }: AuthModalProps) {
  const { login, register } = useAuth()
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [open, setOpen] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [name, setName] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [address, setAddress] = useState('')
  const { showToast } = useToast()

  const resetForm = () => {
    setName('')
    setPhone('')
    setEmail('')
    setPassword('')
    setConfirmPassword('')
    setAddress('')
  }

  const getErrorMessage = (error: unknown) => {
    if (axios.isAxiosError(error)) {
      const payload = error.response?.data as { message?: string; title?: string } | string | undefined
      if (typeof payload === 'string') {
        return payload
      }

      if (payload?.message) {
        return payload.message
      }

      if (payload?.title) {
        return payload.title
      }
    }

    return 'Đã xảy ra lỗi. Vui lòng thử lại.'
  }

  const handleSubmit = async () => {
    if (!email.trim() || !password.trim()) {
      showToast('Thiếu thông tin', 'Vui lòng nhập email và mật khẩu.', 'error')
      return
    }

    if (mode === 'register' && password !== confirmPassword) {
      showToast('Mật khẩu không khớp', 'Vui lòng kiểm tra lại mật khẩu xác nhận.', 'error')
      return
    }

    setIsSubmitting(true)
    try {
      if (mode === 'login') {
        await login(email.trim(), password)
      } else {
        await register({
          email: email.trim(),
          password,
          fullName: name.trim() || undefined,
          phoneNumber: phone.trim() || undefined,
          guestId: getOrCreateGuestId() ?? undefined,
        })
      }

      showToast(
        mode === 'login' ? 'Đăng nhập thành công' : 'Đăng ký thành công',
        mode === 'login' ? 'Chào mừng bạn trở lại với Andev EShop!' : 'Tài khoản của bạn đã được tạo thành công.',
        'success',
      )
      setOpen(false)
      resetForm()
    } catch (error) {
      const errorMessage = getErrorMessage(error)
      showToast(mode === 'login' ? 'Đăng nhập thất bại' : 'Đăng ký thất bại', errorMessage, 'error')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger asChild>{trigger}</Dialog.Trigger>
      <Dialog.Portal>
        <Dialog.Overlay className="radix-overlay" />
        <Dialog.Content className="radix-modal" data-mode={mode}>
          <Dialog.Close className="modal-close-btn icon-button" aria-label="Đóng">
            ✕
          </Dialog.Close>
          <div className="modal-layout">
            <div className="modal-left">
              <form
                className={`modal-form ${mode}`}
                onSubmit={(event) => {
                  event.preventDefault()
                  void handleSubmit()
                }}
              >
                <div className="form-grid">
                  {mode === 'register' && (
                    <>
                      <label>
                        Tên (tuỳ chọn)
                        <input
                          type="text"
                          placeholder="Tên hiển thị"
                          value={name}
                          onChange={(event) => setName(event.target.value)}
                        />
                      </label>
                      <label>
                        Số điện thoại (tuỳ chọn)
                        <input
                          type="tel"
                          placeholder="Số điện thoại"
                          value={phone}
                          onChange={(event) => setPhone(event.target.value)}
                        />
                      </label>
                    </>
                  )}
                  <label>
                    Email
                    <input
                      type="email"
                      placeholder="you@example.com"
                      value={email}
                      onChange={(event) => setEmail(event.target.value)}
                    />
                  </label>
                  <label>
                    Mật khẩu
                    <input
                      type="password"
                      placeholder="••••••••"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                    />
                  </label>
                  {mode === 'register' && (
                    <>
                      <label>
                        Xác nhận mật khẩu
                        <input
                          type="password"
                          placeholder="••••••••"
                          value={confirmPassword}
                          onChange={(event) => setConfirmPassword(event.target.value)}
                        />
                      </label>
                      <label>
                        Địa chỉ (tuỳ chọn)
                        <input
                          type="text"
                          placeholder="Địa chỉ giao hàng"
                          value={address}
                          onChange={(event) => setAddress(event.target.value)}
                        />
                      </label>
                    </>
                  )}
                </div>

                {mode === 'login' && (
                  <button className="link-button" type="button">
                    Quên mật khẩu?
                  </button>
                )}

                <button className="btn btn-primary" type="submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Đang xử lý...' : mode === 'login' ? 'Đăng nhập' : 'Tạo tài khoản'}
                </button>
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
                  onClick={() => {
                    setMode('login')
                    setConfirmPassword('')
                    setAddress('')
                  }}
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
