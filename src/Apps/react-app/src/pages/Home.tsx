import { Link } from 'react-router'
import { useState, useEffect } from 'react'

const HERO_IMAGES = [
    "https://images.unsplash.com/photo-1463107971871-fbac9ddb920f?auto=format&fit=crop&w=800&q=80", // Tech/Shoe
    "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=800&q=80", // Fashion
    "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=800&q=80", // Neon/Tech
    "https://images.unsplash.com/photo-1552346154-21d32810aba3?auto=format&fit=crop&w=800&q=80"  // Sneakers
]

function Home() {
    const [currentSlide, setCurrentSlide] = useState(0)

    useEffect(() => {
        const interval = setInterval(() => {
            setCurrentSlide((prev) => (prev + 1) % HERO_IMAGES.length)
        }, 4000)
        return () => clearInterval(interval)
    }, [])

    return (
        <>
            <header>
                <div>
                    <p className="hero-kicker">BST Tết Nguyên Đán 2026</p>
                    <h1>
                        <span>Lunar Signature</span> Tuyển chọn bởi Andev Original
                    </h1>
                    <p className="hero-text">
                        Thiết kế độc đáo, chất liệu thượng hạng, trải nghiệm giàu cảm hứng.
                    </p>
                    <div className="hero-cta">
                        <Link to="/products" className="btn btn-primary">Khám phá ngay</Link>
                        <button className="btn btn-ghost" type="button">Liên hệ tư vấn</button>
                    </div>
                </div>
                <div className="hero-visual">
                    <div className="hero-slideshow">
                        {HERO_IMAGES.map((src, index) => {
                            // Calculate position relative to currentSlide
                            // 0 = active, 1 = next, 2 = next next, others hidden
                            const length = HERO_IMAGES.length;
                            const diff = (index - currentSlide + length) % length;

                            let slideClass = '';
                            if (diff === 0) slideClass = 'slide-active';
                            else if (diff === 1) slideClass = 'slide-next';
                            else if (diff === 2) slideClass = 'slide-next-2';

                            return (
                                <img
                                    key={index}
                                    src={src}
                                    alt={`Hero visual ${index + 1}`}
                                    className={`hero-slide-img ${slideClass}`}
                                />
                            )
                        })}
                    </div>

                    <div className="hero-controls">
                        <button
                            onClick={() => setCurrentSlide((prev) => (prev - 1 + HERO_IMAGES.length) % HERO_IMAGES.length)}
                            className="control-btn prev"
                            aria-label="Previous slide"
                        >
                            ❮
                        </button>
                        <button
                            onClick={() => setCurrentSlide((prev) => (prev + 1) % HERO_IMAGES.length)}
                            className="control-btn next"
                            aria-label="Next slide"
                        >
                            ❯
                        </button>
                    </div>

                    <div className="hero-indicators">
                        {HERO_IMAGES.map((_, index) => (
                            <button
                                key={index}
                                className={`indicator-dot ${index === currentSlide ? 'active' : ''}`}
                                onClick={() => setCurrentSlide(index)}
                                aria-label={`Go to slide ${index + 1}`}
                            />
                        ))}
                    </div>
                </div>
            </header>

            <div className="stats" style={{ marginBottom: '4rem' }}>
                <div className="stat-item">
                    <h3>36+</h3>
                    <p>Nhà thiết kế độc quyền</p>
                </div>
                <div className="stat-item">
                    <h3>3.6/5</h3>
                    <p>Mức độ hài lòng</p>
                </div>
                <div className="stat-item">
                    <h3>36h</h3>
                    <p>Giao nhanh toàn quốc</p>
                </div>
            </div>

            <div className="floating-notice">
                <span>
                    <strong>Cosmic Friday</strong> | Freeship toàn quốc + ưu đãi 15% smartwear.
                </span>
            </div>

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
