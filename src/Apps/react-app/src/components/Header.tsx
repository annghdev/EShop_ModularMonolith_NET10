import { useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router'
import CartDrawer from './CartDrawer'
import AuthModal from './AuthModal'
import * as DropdownMenu from '@radix-ui/react-dropdown-menu'
import logo from '../assets/logo.png'
import { useAuth } from '../hooks/useAuth'
import { useCart } from '../hooks/useCart'

const BASE_THEME_KEY = 'gptcodex-theme-base'
function Header() {
  const [baseTheme, setBaseTheme] = useState<'light' | 'dark'>(() => {
    const stored = localStorage.getItem(BASE_THEME_KEY)
    return stored === 'light' ? 'light' : 'dark'
  })
  const [isScrolled, setIsScrolled] = useState(false)
  const [isHidden, setIsHidden] = useState(false)
  const lastScrollY = useRef(0)
  const scrollDirection = useRef<'up' | 'down'>('up')
  const isMouseTop = useRef(false)

  const { isAuthenticated, userInfo, logout } = useAuth()
  const { summary } = useCart()
  const cartSelectedCount = summary?.TotalQuantity ?? 0
  const initials = useMemo(() => {
    const displayName = userInfo?.displayName?.trim()
    if (!displayName) {
      return 'U'
    }

    const parts = displayName.split(/\s+/).filter(Boolean)
    if (parts.length === 1) {
      return parts[0].slice(0, 2).toUpperCase()
    }

    return `${parts[0][0] ?? ''}${parts[parts.length - 1][0] ?? ''}`.toUpperCase()
  }, [userInfo?.displayName])

  const handleLogout = async () => {
    await logout()
  }


  // Controlled Dropdown State
  const [isProductsOpen, setIsProductsOpen] = useState(false)
  // Use a ref to keep track of timeout to clear it properly
  const productsTimeoutRef = useState<{ current: ReturnType<typeof setTimeout> | null }>({ current: null })[0]

  const handleProductsEnter = () => {
    if (productsTimeoutRef.current) {
      clearTimeout(productsTimeoutRef.current)
      productsTimeoutRef.current = null
    }
    setIsProductsOpen(true)
  }

  const handleProductsLeave = () => {
    productsTimeoutRef.current = setTimeout(() => {
      setIsProductsOpen(false)
    }, 300)
  }

  useEffect(() => {
    document.body.classList.toggle('light-theme', baseTheme === 'light')
  }, [baseTheme])

  useEffect(() => {
    const handleScroll = () => {
      const currentScrollY = window.scrollY
      setIsScrolled(currentScrollY > 20)

      // Determine scroll direction
      if (currentScrollY > lastScrollY.current) {
        scrollDirection.current = 'down'
      } else if (currentScrollY < lastScrollY.current) {
        scrollDirection.current = 'up'
      }

      // Hide logic: Hide if (Deep Scroll) AND (Going Down) AND (Mouse Not Peeking)
      if (currentScrollY > 100 && scrollDirection.current === 'down' && !isMouseTop.current) {
        setIsHidden(true)
      } else {
        // Show if (Top of page) OR (Going Up) OR (Mouse Peeking)
        setIsHidden(false)
      }

      lastScrollY.current = currentScrollY
    }

    const handleMouseMove = (e: MouseEvent) => {
      const isTop = e.clientY < 80 // Peek threshold
      isMouseTop.current = isTop

      if (isTop) {
        setIsHidden(false)
      } else {
        // If mouse leaves top area, verify if we should revert to hidden (scrolled down state)
        if (window.scrollY > 100 && scrollDirection.current === 'down') {
          setIsHidden(true)
        }
      }
    }

    window.addEventListener('scroll', handleScroll)
    window.addEventListener('mousemove', handleMouseMove)

    return () => {
      window.removeEventListener('scroll', handleScroll)
      window.removeEventListener('mousemove', handleMouseMove)
    }
  }, [])

  const baseThemeLabel = useMemo(
    () => (baseTheme === 'light' ? (
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <circle cx="12" cy="12" r="4" />
        <path d="M12 2v2" />
        <path d="M12 20v2" />
        <path d="m4.93 4.93 1.41 1.41" />
        <path d="m17.66 17.66 1.41 1.41" />
        <path d="M2 12h2" />
        <path d="M20 12h2" />
        <path d="m6.34 17.66-1.41 1.41" />
        <path d="m19.07 4.93-1.41 1.41" />
      </svg>
    ) : (
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z" />
      </svg>
    )),
    [baseTheme],
  )

  const toggleBaseTheme = () => {
    const next = baseTheme === 'light' ? 'dark' : 'light'
    setBaseTheme(next)
    localStorage.setItem(BASE_THEME_KEY, next)
  }

  return (
    <>
      <nav className={`top-nav${isScrolled ? ' scrolled' : ''}${isHidden ? ' hidden' : ''}`}>
        <Link to="/" className="logo">
          <img src={logo} alt="Andev Original" className="logo-icon" />
          Andev Original
        </Link>
        <div className="nav-links">
          <DropdownMenu.Root open={isProductsOpen} onOpenChange={setIsProductsOpen} modal={false}>
            <DropdownMenu.Trigger asChild>
              <div
                onMouseEnter={handleProductsEnter}
                onMouseLeave={handleProductsLeave}
                style={{ height: '100%', display: 'flex', alignItems: 'center', outline: 'none' }}
              >
                <Link
                  to="/products"
                  className="nav-link-trigger"
                  onClick={(e) => {
                    e.stopPropagation()
                    setIsProductsOpen(false)
                  }}
                >
                  Sản phẩm
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="m6 9 6 6 6-6" />
                  </svg>
                </Link>
              </div>
            </DropdownMenu.Trigger>
            <DropdownMenu.Portal>
              <DropdownMenu.Content
                className="dropdown-menu composite"
                sideOffset={25}
                align="start"
                onMouseEnter={handleProductsEnter}
                onMouseLeave={handleProductsLeave}
              >
                <div className="mega-menu-grid">
                  <div className="mega-column">
                    <h3>Danh mục</h3>
                    <DropdownMenu.Sub>
                      <DropdownMenu.SubTrigger className="dropdown-sub-trigger">
                        Giày thể thao <span>›</span>
                      </DropdownMenu.SubTrigger>
                      <DropdownMenu.Portal>
                        <DropdownMenu.SubContent className="dropdown-sub-content" sideOffset={2} alignOffset={-5}>
                          <DropdownMenu.Item className="dropdown-item" asChild>
                            <Link to="/products?category=Running">Chạy bộ</Link>
                          </DropdownMenu.Item>
                          <DropdownMenu.Item className="dropdown-item" asChild>
                            <Link to="/products?category=Basketball">Bóng rổ</Link>
                          </DropdownMenu.Item>
                          <DropdownMenu.Item className="dropdown-item" asChild>
                            <Link to="/products?category=Casual">Casual</Link>
                          </DropdownMenu.Item>
                        </DropdownMenu.SubContent>
                      </DropdownMenu.Portal>
                    </DropdownMenu.Sub>

                    <DropdownMenu.Sub>
                      <DropdownMenu.SubTrigger className="dropdown-sub-trigger">
                        Quần áo <span>›</span>
                      </DropdownMenu.SubTrigger>
                      <DropdownMenu.Portal>
                        <DropdownMenu.SubContent className="dropdown-sub-content" sideOffset={2} alignOffset={-5}>
                          <DropdownMenu.Item className="dropdown-item" asChild>
                            <Link to="/products?category=T-Shirt">Áo thun</Link>
                          </DropdownMenu.Item>
                          <DropdownMenu.Item className="dropdown-item" asChild>
                            <Link to="/products?category=Jacket">Áo khoác</Link>
                          </DropdownMenu.Item>
                          <DropdownMenu.Item className="dropdown-item" asChild>
                            <Link to="/products?category=Pants">Quần</Link>
                          </DropdownMenu.Item>
                        </DropdownMenu.SubContent>
                      </DropdownMenu.Portal>
                    </DropdownMenu.Sub>

                    <Link to="/products?category=Accessories" className="dropdown-sub-trigger">Phụ kiện</Link>
                  </div>
                  <div className="mega-column">
                    <h3>Thương hiệu</h3>
                    <Link to="/products?brand=Nike" className="mega-item">Nike</Link>
                    <Link to="/products?brand=Adidas" className="mega-item">Adidas</Link>
                    <Link to="/products?brand=Puma" className="mega-item">Puma</Link>
                  </div>
                  <div className="mega-column featured">
                    <div className="mega-promo">
                      <span>New Arrival</span>
                      <h4>Summer Collection 2026</h4>
                    </div>
                  </div>
                </div>
              </DropdownMenu.Content>
            </DropdownMenu.Portal>
          </DropdownMenu.Root>
          <Link to="/products?featured=sale">Khuyến mãi</Link>
          <Link to="/products?featured=hot">Nổi bật</Link>
          <Link to="/products?collection=signature">Bộ sưu tập</Link>
        </div>
        <div className="nav-actions">
          <div className="header-search">
            <button className="search-btn" aria-label="Tìm kiếm">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="11" cy="11" r="8" />
                <path d="m21 21-4.3-4.3" />
              </svg>
            </button>
            <input type="text" placeholder="Tìm kiếm sản phẩm..." />
          </div>
          <div className="theme-icon-group" role="group" aria-label="Tùy chọn giao diện">
            <button
              className={`icon-toggle${baseTheme === 'light' ? ' active' : ''}`}
              id="baseThemeToggle"
              type="button"
              aria-label={baseTheme === 'light' ? 'Chuyển sang chế độ tối' : 'Chuyển sang chế độ sáng'}
              onClick={toggleBaseTheme}
            >
              {baseThemeLabel}
            </button>
          </div>
          <CartDrawer
            trigger={(
              <button className="icon-button cart-button" type="button" aria-label="Giỏ hàng">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="8" cy="21" r="1" />
                  <circle cx="19" cy="21" r="1" />
                  <path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />
                </svg>
                {cartSelectedCount > 0 && (
                  <span className="badge-count">{cartSelectedCount}</span>
                )}
              </button>
            )}
          />
          {isAuthenticated ? (
            <>
              <button className="icon-button" type="button" aria-label="Thông báo">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
                  <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" />
                </svg>
                <span className="badge-count">6</span>
              </button>
              <DropdownMenu.Root>
                <DropdownMenu.Trigger asChild>
                  <button className="avatar-button" type="button" aria-label="Tài khoản">
                    <div className="avatar-initials">{initials}</div>
                    <svg className="avatar-chevron" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                      <path d="m6 9 6 6 6-6" />
                    </svg>
                  </button>
                </DropdownMenu.Trigger>
                <DropdownMenu.Portal>
                  <DropdownMenu.Content className="dropdown-menu" sideOffset={14} align="end">
                    <DropdownMenu.Item className="dropdown-item" asChild>
                      <Link to="/profile#vouchers">Kho voucher</Link>
                    </DropdownMenu.Item>
                    <DropdownMenu.Item className="dropdown-item" asChild>
                      <Link to="/history">Lịch sử mua hàng</Link>
                    </DropdownMenu.Item>
                    <DropdownMenu.Item className="dropdown-item" asChild>
                      <Link to="/profile">Hồ sơ cá nhân</Link>
                    </DropdownMenu.Item>
                    <DropdownMenu.Separator className="dropdown-separator" />
                    <DropdownMenu.Item
                      className="dropdown-item danger"
                      onSelect={() => { void handleLogout() }}
                    >
                      Đăng xuất
                    </DropdownMenu.Item>
                  </DropdownMenu.Content>
                </DropdownMenu.Portal>
              </DropdownMenu.Root>
            </>
          ) : (
            <AuthModal
              trigger={(
                <button className="chip ghost" type="button">
                  Đăng nhập
                </button>
              )}
            />
          )}
        </div>
      </nav>
      <div className="garland" aria-hidden="true" />
    </>
  )
}

export default Header
