## GPT Codex Boutique – Style Guide

### 1. Brand Essence
- **Tone**: Tương lai, sang trọng, giàu cảm xúc; kết hợp yếu tố công nghệ và thời trang thủ công cao cấp.
- **Moodwords**: Glassmorphism, gradient nebula, curated boutique, midnight luxe, holiday shimmer (Noel mode).

### 2. Color System
| Token | Default | Light Theme | Noel Dark | Noel Light | Use |
|-------|---------|-------------|-----------|------------|-----|
| `--bg-dark` | `#05070f` | `#f7f8fb` | `#050b14` | `#fff7f2` | Page background |
| `--bg-card` | `rgba(15,18,35,0.72)` | `rgba(255,255,255,0.9)` | `rgba(14,21,32,0.9)` | `rgba(255,255,255,0.96)` | Cards, glass panels |
| `--accent` | `#f9b16e` | `#e27b36` | `#f45d48` | `#e6544f` | Primary highlights |
| `--accent-2` | `#8f41ff` | `#6078ea` | `#58d6a0` | `#48b087` | Gradient pair / CTA |
| `--glass-border` | `rgba(255,255,255,0.08)` | `rgba(13,16,34,0.08)` | `rgba(255,255,255,0.15)` | `rgba(244,93,72,0.2)` | Borders for glass blocks |
| `--text` | `#f5f5f7` | `#0d1022` | `#fdfcf7` | `#2b1c1a` | Body text |
| `--muted` | `#b4b4c7` | `#52556a` | `#d7e2ff` | `#735046` | Secondary text |
| `--footer-bg` | `rgba(7,9,20,0.75)` | `rgba(255,255,255,0.92)` | `rgba(5,9,16,0.9)` | `rgba(255,255,255,0.94)` | Footer glass |
| Noel tokens (`--noel-*`) | Xem `products_by_gptcodex_v3.html` | Controls chip, search, card, pagination & snowfall colors for Noel (dark + light). |

### 3. Typography
- **Headings**: `Playfair Display` 600–700, dùng `clamp` cho hero (`clamp(2.7rem, 4vw, 4rem)`).
- **Body & UI**: `Poppins` 400–700.
- **Letter spacing**: hero kicker `0.3–0.35em`, category pill `0.2–0.35em`.
- **Transforms**: uppercase for navigational text, kicker, category labels.

### 4. Layout & Grid
- **Page width**: `min(1200px, 94vw)` with `padding: 70px 0 120px`.
- **Sections**: hero (2-column auto-fit), filter toolbar (flex wrap), cards grid `repeat(auto-fill, minmax(260px, 1fr))`.
- **Responsive**: below 640px: stack hero columns, pills, layout toggle full width, product grid single column (opt-in 2-col mobile toggle).
- **Z layers**: nebula (z0) < snow (z1) < content `.page` (z3) để tuyết không đè UI.

### 5. Components
#### 5.1 Navigation
- Glass nav with sticky top, gradient brand badge, icon buttons:
  - **Base theme toggle**: `#baseThemeToggle` (moon/sun icon) switches dark/light via `localStorage`.
  - **Holiday toggle**: `#holidayToggle` (sparkle/tree icon) toggles Noel overlay, works with either base theme.
  - Icon buttons: 44×44px, circular, `linear-gradient` when active.

#### 5.2 Pills & Chips
- Base: glass pill with `border-radius: 999px`, `transition: 0.25s`.
- Active state: gradient fill `linear-gradient(120deg, accent, accent-2)` + `box-shadow`.
- Noel tokens override colors via `--noel-chip-*`.

#### 5.3 Cards
- Product cards: 30px radius (28px Noel), `backdrop-filter: blur(18px)`, hover lift (`translateY(-10px)` default, `-6px` light theme).
- Include favorite badge (`.favorite`), media figure (200px height), metadata, CTA duo.
- Noel mode adds dashed inner border overlay and curated gradient backgrounds.

#### 5.4 Buttons
- Primary CTA: gradient `linear-gradient(120deg, accent, accent-2)`, `box-shadow` for depth.
- Secondary CTA: glass background, lighten/darken on hover; Noel overrides use `--noel-secondary-btn-*`.
- Micro interactions: hover lifts `-2px`, active pushes `+2px`.

#### 5.5 Filters / Search
- Search input & sort select share pill shape, leading icon via pseudo span.
- Layout toggle pill toggles `.mobile-two-cols` class on grid.

### 6. Effects
- **Glassmorphism**: consistent `border: 1px solid var(--glass-border)` + `backdrop-filter: blur(18px)`.
- **Nebula**: 3 radial gradients with `drift` animation, `pointer-events:none`.
- **Snowfall**: `.snow-layer` fixed overlay, `snowFall` animation; Noel light uses larger colorful flakes (`4–6px`).
- **Garland**: decorative gradient line and twinkling bulbs, only visible in Noel mode.

### 7. Interaction States
- Hover: subtle lift (`translateY(-2px/-3px)`), gradient buttons brighten.
- Active: `translateY(2px)` to mimic press.
- Favorite toggle: `box-shadow` glow, color shift to accent.
- Pagination: active page uses gradient fill; disabled buttons drop opacity.

### 8. Theming Rules
1. Base theme (dark/light) controls global background + neutrals.
2. Holiday mode overlays Noel palette on top of current base theme via CSS variables.
3. Both toggles persist using `localStorage` keys `gptcodex-theme-base` & `gptcodex-theme-holiday`.
4. Snow layer opacity & colors adapt to theme; ensure `pointer-events:none` and correct z-index layering.

### 9. Content Guidelines
- Tone ngắn gọn, giàu cảm hứng, mô tả vật liệu/công nghệ.
- Dùng emoji nhẹ nhàng (hero pill ✦, mood cards 🌌/🌿/⚡, Noel icon 🎄) để nhấn mạnh cảm xúc.
- Giữ headline Vi/En kết hợp (ví dụ “The Curated Edit”, “Bộ sưu tập Siêu phẩm”).

### 10. Assets & References
- Primary layout & components: `e-commerce/products_by_gptcodex_v3.html`.
- Token overrides & JS theme logic: same file, section cuối `<script>`.
- Nội dung demo sản phẩm: mảng `products` trong script (10 items).

