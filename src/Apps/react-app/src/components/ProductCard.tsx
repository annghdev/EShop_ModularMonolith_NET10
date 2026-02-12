import { useMemo } from 'react'
import { useNavigate } from 'react-router'
import { useToast } from './Toast'

type VariantDotValue = {
  Id: string
  Value: string
  ColorCode?: string | null
}

type VariantDot = {
  AttributeName: string
  DisplayType: string
  ValueStyleCss?: string | null
  Values: VariantDotValue[]
}

export type ProductCardData = {
  Id: string
  Name: string
  Description?: string | null
  Slug: string
  OriginalPrice: number
  DiscountPercent: number
  DiscountedPrice: number
  Currency: string
  Rating: number
  SoldCount: number
  FeedbackCount: number
  FeaturedTag: string
  Thumbnail?: string | null
  SecondaryImage?: string | null
  VariantDots?: VariantDot[]
}

type ProductCardProps = {
  product: ProductCardData
  imageUrl: string
}

const COLOR_ALIASES: Record<string, string> = {
  black: '#111827',
  white: '#f5f5f7',
  gray: '#9ca3af',
  grey: '#9ca3af',
  silver: '#cbd5f5',
  gold: '#f2c94c',
  beige: '#d6c2a6',
  brown: '#8b5e34',
  red: '#ef4444',
  orange: '#f97316',
  yellow: '#facc15',
  green: '#22c55e',
  teal: '#14b8a6',
  blue: '#3b82f6',
  navy: '#1e3a8a',
  purple: '#8b5cf6',
  pink: '#ec4899',
}

const isValidCssColor = (value: string) => {
  const style = new Option().style
  style.color = value
  return style.color !== ''
}

const normalizeColor = (value: string) => {
  const normalized = value.trim().toLowerCase()
  if (COLOR_ALIASES[normalized]) {
    return COLOR_ALIASES[normalized]
  }
  if (isValidCssColor(normalized)) {
    return normalized
  }
  return '#d1d5db'
}

const getColorValues = (product: ProductCardData) => {
  const colors = new Map<string, string>()
  product.VariantDots?.forEach((dot) => {
    const isColor =
      dot.DisplayType?.toLowerCase() === 'color' ||
      dot.AttributeName?.toLowerCase() === 'color'
    if (!isColor) return
    dot.Values?.forEach((value) => {
      const key = value.Value?.trim()
      if (!key) return
      const colorValue = value.ColorCode?.trim()
      if (!colors.has(key)) {
        colors.set(key, colorValue ? normalizeColor(colorValue) : normalizeColor(key))
      }
    })
  })
  return Array.from(colors.entries()).map(([name, value]) => ({ name, value }))
}

function ProductCard({ product, imageUrl }: ProductCardProps) {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const colorDots = useMemo(() => getColorValues(product), [product])
  const textDots = useMemo(() => {
    return (
      product.VariantDots?.filter((dot) => {
        const type = dot.DisplayType?.toLowerCase()
        const name = dot.AttributeName?.toLowerCase()
        return type === 'text' || (type !== 'color' && name !== 'color')
      }) ?? []
    )
  }, [product.VariantDots])

  const formattedPrice = useMemo(() => {
    const currency = product.Currency || 'VND'
    try {
      return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency,
      }).format(product.DiscountedPrice)
    } catch {
      return `${product.DiscountedPrice.toLocaleString('vi-VN')} ${currency}`
    }
  }, [product.DiscountedPrice, product.Currency])

  const formattedOriginalPrice = useMemo(() => {
    if (!product.OriginalPrice) return null
    const currency = product.Currency || 'VND'
    try {
      return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency,
      }).format(product.OriginalPrice)
    } catch {
      return `${product.OriginalPrice.toLocaleString('vi-VN')} ${currency}`
    }
  }, [product.OriginalPrice, product.Currency])

  const featuredTag = product.FeaturedTag?.toLowerCase()
  const showFeatured = featuredTag && featuredTag !== 'none'

  return (
    <article className="product-card">
      <figure className="product-media">
        <img src={imageUrl} alt={product.Name} loading="lazy" />
        {product.SecondaryImage && (
          <img
            src={product.SecondaryImage}
            alt={`${product.Name} alternate`}
            className="secondary"
            loading="lazy"
          />
        )}
        {showFeatured && (
          <span className={`badge ${featuredTag}`}>{product.FeaturedTag}</span>
        )}
        <button className="btn-wishlist" aria-label="Thêm vào yêu thích">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path></svg>
        </button>
      </figure>
      <h3 className="product-title">{product.Name}</h3>
      {/* <p className="product-desc">
        {product.Description || 'Thiết kế tinh tế, vật liệu cao cấp, nâng tầm trải nghiệm.'}
      </p> */}
      {colorDots.length > 0 && (
        <div className="product-colors" aria-label="Tùy chọn màu sắc">
          {colorDots.map((color) => (
            <span
              key={`${product.Id}-${color.name}`}
              className="color-dot"
              style={{ backgroundColor: color.value }}
              title={color.name}
            />
          ))}
        </div>
      )}
      {textDots.length > 0 && (
        <div className="variant-groups" aria-label="Tùy chọn biến thể">
          {textDots.map((dot) => (
            <div key={`${product.Id}-${dot.AttributeName}`} className="variant-group">
              <span className="variant-label">{dot.AttributeName}</span>
              <div className="variant-tags inline">
                {dot.Values.map((value) => (
                  <span
                    key={`${product.Id}-${dot.AttributeName}-${value.Id}`}
                    className="variant-tag"
                  >
                    {value.Value}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
      <div className="product-meta">
        <div className="price">
          {formattedPrice}
          <div className="price-row">
            {formattedOriginalPrice && formattedOriginalPrice !== formattedPrice && (
              <small className="original">{formattedOriginalPrice}</small>
            )}
            {product.DiscountPercent > 0 && (
              <span className="discount-tag">-{product.DiscountPercent}%</span>
            )}
          </div>
        </div>
        <div className="rating">★ {product.Rating.toFixed(1)}</div>
      </div>
      <div className="product-stats">
        <span>{product.SoldCount.toLocaleString('vi-VN')} đã bán</span>
        <span>({product.FeedbackCount}) đánh giá</span>
      </div>
      <div className="product-actions">
        <button
          className="secondary"
          type="button"
          onClick={() => navigate(`/products/${product.Slug}`)}
        >
          Xem chi tiết
        </button>
        <button
          className="primary"
          type="button"
          onClick={() => {
            showToast('Chọn biến thể', 'Vui lòng chọn biến thể tại trang chi tiết trước khi thêm vào giỏ.', 'info')
            navigate(`/products/${product.Slug}`)
          }}
        >
          Thêm giỏ
        </button>
      </div>
    </article>
  )
}

export default ProductCard
