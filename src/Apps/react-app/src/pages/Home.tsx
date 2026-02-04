import { Link } from 'react-router'

function Home() {
    return (
        <>
            <div className="floating-notice">
                <span>
                    <strong>Cosmic Friday</strong> | Freeship toàn quốc + ưu đãi 15% smartwear.
                </span>
            </div>

            <header>
                <div>
                    <p className="hero-kicker">The Curated Edit</p>
                    <h1>
                        Bộ sưu tập <span>Siêu phẩm</span> được tuyển chọn bởi Andev Original Boutique
                    </h1>
                    <p className="hero-text">
                        Thiết kế tương lai, chất liệu thượng hạng, trải nghiệm giàu cảm hứng.
                    </p>
                    <div className="hero-cta">
                        <Link to="/products" className="btn btn-primary">Khám phá ngay</Link>
                        <button className="btn btn-ghost" type="button">Trải nghiệm AR</button>
                    </div>
                    <div className="stats">
                        <div className="stat-item">
                            <h3>120+</h3>
                            <p>Nhà thiết kế độc quyền</p>
                        </div>
                        <div className="stat-item">
                            <h3>4.9/5</h3>
                            <p>Mức độ hài lòng</p>
                        </div>
                        <div className="stat-item">
                            <h3>48h</h3>
                            <p>Giao nhanh toàn quốc</p>
                        </div>
                    </div>
                </div>
                <div className="hero-visual">
                    <div className="hero-pill">Bộ sưu tập Signature ✦</div>
                    <img
                        src="https://images.unsplash.com/photo-1463107971871-fbac9ddb920f?auto=format&fit=crop&w=800&q=80"
                        alt="Hero product"
                    />
                </div>
            </header>

            <section>
                <p className="section-heading">Moodboard hôm nay</p>
                <div className="mood-row">
                    <div className="mood-card">
                        <div>
                            <strong>Chill Neon</strong>
                            <p>Gam màu gradient ngọt ngào</p>
                        </div>
                        <span>🌌</span>
                    </div>
                    <div className="mood-card">
                        <div>
                            <strong>Slow Living</strong>
                            <p>Chạm là mê, cảm giác mịn lì</p>
                        </div>
                        <span>🌿</span>
                    </div>
                    <div className="mood-card">
                        <div>
                            <strong>Night Runner</strong>
                            <p>Phóng khoáng, thời thượng</p>
                        </div>
                        <span>⚡</span>
                    </div>
                </div>
            </section>

            <section className="testimonials">
                <p className="section-heading">Giới mộ điệu nói gì</p>
                <div className="testimonial-grid">
                    <article className="testimonial-card">
                        <div className="testimonial-avatar">
                            <span>LT</span>
                            <div>
                                <strong>Linh Trần</strong>
                                <p>Creative Director</p>
                            </div>
                        </div>
                        <p>"Layer smartwear tại Andev Original có texture cực kỳ mượt, phối cùng sneakers Pulse thì lên outfit đúng chất future-chic."</p>
                        <div className="rating">★ 4.98</div>
                    </article>
                    <article className="testimonial-card">
                        <div className="testimonial-avatar">
                            <span>NA</span>
                            <div>
                                <strong>Nam Anh</strong>
                                <p>Sound Artist</p>
                            </div>
                        </div>
                        <p>"Nebula Pods cân được cả studio di động của tôi. Spatial audio chân thật, đặt hàng có stylist hỗ trợ luôn."</p>
                        <div className="rating">★ 4.95</div>
                    </article>
                    <article className="testimonial-card">
                        <div className="testimonial-avatar">
                            <span>MH</span>
                            <div>
                                <strong>Minh Hà</strong>
                                <p>Interior Curator</p>
                            </div>
                        </div>
                        <p>"Bộ Kaihō ceramic đi cùng Velvet Glow Lamp khiến góc đọc tuyệt đối thư thái. Giao trong 36h rất ấn tượng."</p>
                        <div className="rating">★ 5.0</div>
                    </article>
                </div>
            </section>
        </>
    )
}

export default Home
