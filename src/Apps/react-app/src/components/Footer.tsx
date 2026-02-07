import { Link } from 'react-router'

function Footer() {
  return (
    <footer className="site-footer">
      <div className="footer-grid">
        <div className="footer-brand">
          <Link to="/">Andev Original</Link>
          <p>Thời thượng - bền vững</p>
          <p>Trao niềm tin - nhận tài lộc</p>
          <br />
          <div className="footer-socials">
            <a href="https://instagram.com" target="_blank" rel="noopener noreferrer" aria-label="Instagram">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="20" height="20" x="2" y="2" rx="5" ry="5" /><path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z" /><line x1="17.5" x2="17.51" y1="6.5" y2="6.5" /></svg>
            </a>
            <a href="https://facebook.com" target="_blank" rel="noopener noreferrer" aria-label="Facebook">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z" /></svg>
            </a>
            <a href="https://tiktok.com" target="_blank" rel="noopener noreferrer" aria-label="TikTok">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 12a4 4 0 1 0 4 4V4a5 5 0 0 0 5 5" /></svg>
            </a>
            <a href="https://youtube.com" target="_blank" rel="noopener noreferrer" aria-label="YouTube">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M2.5 17a24.12 24.12 0 0 1 0-10 2 2 0 0 1 1.4-1.4 49.56 49.56 0 0 1 16.2 0A2 2 0 0 1 21.5 7a24.12 24.12 0 0 1 0 10 2 2 0 0 1-1.4 1.4 49.55 49.55 0 0 1-16.2 0A2 2 0 0 1 2.5 17" /><path d="m10 15 5-3-5-3z" /></svg>
            </a>
          </div>
        </div>
        <div>
          <p className="footer-heading">DANH MỤC</p>
          <div className="footer-links-group">
            <div className="footer-sub-group">
              <strong>Giày thể thao</strong>
              <ul className="footer-links">
                <li><Link to="/products?category=Running">Chạy bộ</Link></li>
                <li><Link to="/products?category=Basketball">Bóng rổ</Link></li>
                <li><Link to="/products?category=Casual">Casual</Link></li>
              </ul>
            </div>
            <div className="footer-sub-group">
              <strong>Quần áo</strong>
              <ul className="footer-links">
                <li><Link to="/products?category=T-Shirt">Áo thun</Link></li>
                <li><Link to="/products?category=Jacket">Áo khoác</Link></li>
                <li><Link to="/products?category=Pants">Quần</Link></li>
              </ul>
            </div>
            <div className="footer-sub-group">
              <strong>Phụ kiện</strong>
              <ul className="footer-links">
                <li><Link to="/products?category=Accessories">Tất cả phụ kiện</Link></li>
              </ul>
            </div>
          </div>
        </div>
        <div>
          <p className="footer-heading">THƯƠNG HIỆU</p>
          <ul className="footer-links">
            <li><Link to="/products?brand=Nike">Nike</Link></li>
            <li><Link to="/products?brand=Adidas">Adidas</Link></li>
            <li><Link to="/products?brand=Puma">Puma</Link></li>
            <li><Link to="/products?brand=Andev">Andev Original</Link></li>
          </ul>
        </div>
        <div>
          <p className="footer-heading">VỀ CHÚNG TÔI</p>
          <ul className="footer-links">
            <li><a href="#">Giới thiệu</a></li>
            <li><a href="#">Liên hệ</a></li>
            <li><a href="#">Chính sách bảo mật</a></li>
            {/* <li><a href="#">Điều khoản sử dụng</a></li> */}
            <li><a href="#">Chính sách đổi trả</a></li>
          </ul>
        </div>
      </div>
      <div className="footer-bottom">
        <div className="footer-bottom-left">
          <div className="copyright">
            © 2026 Andev Original
          </div>
          <p className="footer-address">Địa chỉ: 470 Đường Trần Đại Nghĩa, Khu đô thị Đại học Quốc gia TP.HCM</p>
          <p className="footer-address">Hotline: 0867 662 945 &nbsp;|&nbsp; Email: annghdev@gmail.com</p>
        </div>
        <div className="footer-bottom-right">
          <div className="footer-legal">
            Privacy · Terms · Cookies
          </div>
          <div className="payment-group">
            {/* <span className="payment-label">Hỗ trợ thanh toán</span> */}
            <div className="payment-icons">
              <div className="payment-icon momo" title="Momo">
                <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="2" y="4" width="20" height="16" rx="2" /><path d="M12 8v8" /><path d="M8 12h8" /></svg>
              </div>
              <div className="payment-icon vnpay" title="VNPAY">
                <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M4 4l8 16 8-16" /></svg>
              </div>
              <div className="payment-icon zalopay" title="ZaloPay">
                <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10" /><path d="m9 9 6 6" /><path d="m15 9-6 6" /></svg>
              </div>
              <div className="payment-icon bank" title="Bank Transfer">
                <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="2" y="5" width="20" height="14" rx="2" /><line x1="2" y1="10" x2="22" y2="10" /></svg>
              </div>
            </div>
          </div>
        </div>
      </div>
    </footer>
  )
}

export default Footer
