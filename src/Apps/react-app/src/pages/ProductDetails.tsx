import { useEffect, useMemo, useRef, useState, type MouseEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { api, API_PREFIX } from '../config/api'
import { useCart } from '../hooks/useCart'
import { useToast } from '../components/Toast'

type Money = {
  Amount: number
  Currency?: string | null
}

type Dimensions = {
  Width: number
  Height: number
  Depth: number
  Weight: number
}

type Brand = {
  Id: string
  Name: string
  Logo?: string | null
}

type Category = {
  Id: string
  Name: string
}

type VariantAttributeValue = {
  ProductAttributeId: string
  AttributeName: string
  ValueId: string
  ValueName: string
  ColorCode?: string | null
}

type Variant = {
  Id: string
  Name: string
  Sku: string
  OverrideCost?: Money | null
  OverridePrice?: Money | null
  Dimensions?: Dimensions | null
  MainImage?: string | null
  Images: string[]
  AttributeValues: VariantAttributeValue[]
}

type ProductAttribute = {
  AttributeId: string
  AttributeName: string
  DisplayOrder: number
  HasVariant: boolean
}

type Product = {
  Id: string
  Name: string
  Description?: string | null
  Slug?: string | null
  SkuPrefix?: string | null
  Cost?: Money | null
  Price?: Money | null
  Dimensions?: Dimensions | null
  Thumbnail?: string | null
  Images: string[]
  Brand: Brand
  Category: Category
  Attributes: ProductAttribute[]
  Variants: Variant[]
}

type VariantQuantity = {
  VariantId: string
  QuantityAvailable: number
}

const toNumber = (value: unknown, fallback = 0) => {
  const numberValue = typeof value === 'number' ? value : Number(value)
  return Number.isFinite(numberValue) ? numberValue : fallback
}

const normalizeMoney = (raw: unknown): Money | null => {
  if (!raw) return null
  if (typeof raw === 'number' || typeof raw === 'string') {
    const amount = toNumber(raw, NaN)
    if (!Number.isFinite(amount)) return null
    return { Amount: amount }
  }
  const moneyRaw = raw as Record<string, unknown>
  const amount = toNumber(moneyRaw.Amount ?? moneyRaw.amount, NaN)
  if (!Number.isFinite(amount)) return null
  return {
    Amount: amount,
    Currency: (moneyRaw.Currency ?? moneyRaw.currency ?? null) as string | null,
  }
}

const normalizeDimensions = (raw: unknown): Dimensions | null => {
  if (!raw) return null
  const dimRaw = raw as Record<string, unknown>
  return {
    Width: toNumber(dimRaw.Width ?? dimRaw.width, 0),
    Height: toNumber(dimRaw.Height ?? dimRaw.height, 0),
    Depth: toNumber(dimRaw.Depth ?? dimRaw.depth, 0),
    Weight: toNumber(dimRaw.Weight ?? dimRaw.weight, 0),
  }
}

const formatMoney = (money?: Money | null) => {
  if (!money || !Number.isFinite(money.Amount)) return '--'
  const currencyRaw = money.Currency ?? ''
  const currency = currencyRaw.toUpperCase()
  const formatted = money.Amount.toLocaleString('vi-VN', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  })
  if (currency === 'USD' || currency === 'US') {
    return `$${formatted}`
  }
  if (currency) {
    return `${formatted} ${currency}`
  }
  return formatted
}

const getSafeArray = (value: unknown) => (Array.isArray(value) ? value : [])

const uniqueImages = (images: Array<string | null | undefined>) => {
  const set = new Set<string>()
  images.forEach((img) => {
    if (img && img.trim().length > 0) {
      set.add(img)
    }
  })
  return Array.from(set)
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
  if (typeof window === 'undefined') return true
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

function ProductDetails() {
  const { slug } = useParams<{ slug: string }>()
  const navigate = useNavigate()
  const { addItem } = useCart()
  const { showToast } = useToast()
  const [product, setProduct] = useState<Product | null>(null)
  const [variantQuantities, setVariantQuantities] = useState<VariantQuantity[]>([])
  const [selectedAttributes, setSelectedAttributes] = useState<Record<string, string>>({})
  const [selectedVariantId, setSelectedVariantId] = useState<string | null>(null)
  const [selectedImageIndex, setSelectedImageIndex] = useState(0)
  const [hoverLens, setHoverLens] = useState({
    visible: false,
    left: 0,
    top: 0,
    bgX: 0,
    bgY: 0,
    bgWidth: 0,
    bgHeight: 0,
  })
  const [loading, setLoading] = useState(false)
  const [isAddingToCart, setIsAddingToCart] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const imageRef = useRef<HTMLDivElement | null>(null)
  const actionsWrapperRef = useRef<HTMLDivElement | null>(null)
  const actionsRef = useRef<HTMLDivElement | null>(null)
  const [stickyMode, setStickyMode] = useState<'normal' | 'top' | 'bottom'>('normal')
  const [stickyStyle, setStickyStyle] = useState<{ width: number; left: number }>({
    width: 0,
    left: 0,
  })
  const [actionsHeight, setActionsHeight] = useState(0)
  const ZOOM_LEVEL = 2.4
  const getLensSize = () => (typeof window !== 'undefined' && window.innerWidth < 640 ? 180 : 240)
  const [lensSize, setLensSize] = useState(getLensSize)

  useEffect(() => {
    const handleResize = () => setLensSize(getLensSize())
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  useEffect(() => {
    if (!actionsWrapperRef.current || !actionsRef.current) return

    let ticking = false

    const updateStickyPosition = () => {
      if (!actionsWrapperRef.current || !actionsRef.current) return

      const wrapperRect = actionsWrapperRef.current.getBoundingClientRect()
      const viewportHeight = window.innerHeight
      const headerOffset = 120

      const nextMode =
        wrapperRect.top <= headerOffset
          ? 'top'
          : wrapperRect.bottom >= viewportHeight
            ? 'bottom'
            : 'normal'

      const nextHeight = actionsRef.current.offsetHeight
      if (nextHeight && nextHeight !== actionsHeight) {
        setActionsHeight(nextHeight)
      }

      if (nextMode !== 'normal') {
        setStickyStyle({
          width: wrapperRect.width,
          left: wrapperRect.left,
        })
      }

      setStickyMode(nextMode)
      ticking = false
    }

    const handleScroll = () => {
      if (!ticking) {
        window.requestAnimationFrame(updateStickyPosition)
        ticking = true
      }
    }

    const handleResize = () => {
      updateStickyPosition()
    }

    window.addEventListener('scroll', handleScroll, { passive: true })
    window.addEventListener('resize', handleResize)

    updateStickyPosition()

    return () => {
      window.removeEventListener('scroll', handleScroll)
      window.removeEventListener('resize', handleResize)
    }
  }, [product, actionsHeight])

  useEffect(() => {
    if (!slug) return
    const fetchDetails = async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await api.get(`${API_PREFIX}/products/details/${slug}`)
        const raw = response.data ?? {}
        const rawProduct = raw.Product ?? raw.product ?? null
        const rawVariantQuantities = raw.VariantQuantities ?? raw.variantQuantities ?? []

        if (!rawProduct) {
          setProduct(null)
          setVariantQuantities([])
          return
        }

        const mappedProduct: Product = {
          Id: String(rawProduct.Id ?? rawProduct.id ?? ''),
          Name: String(rawProduct.Name ?? rawProduct.name ?? ''),
          Description: (rawProduct.Description ?? rawProduct.description ?? null) as string | null,
          Slug: (rawProduct.Slug ?? rawProduct.slug ?? null) as string | null,
          SkuPrefix: (rawProduct.SkuPrefix ?? rawProduct.skuPrefix ?? null) as string | null,
          Cost: normalizeMoney(rawProduct.Cost ?? rawProduct.cost ?? null),
          Price: normalizeMoney(rawProduct.Price ?? rawProduct.price ?? null),
          Dimensions: normalizeDimensions(rawProduct.Dimensions ?? rawProduct.dimensions ?? null),
          Thumbnail: (rawProduct.Thumbnail ?? rawProduct.thumbnail ?? null) as string | null,
          Images: getSafeArray(rawProduct.Images ?? rawProduct.images).map((img) => String(img)),
          Brand: {
            Id: String(rawProduct.Brand?.Id ?? rawProduct.brand?.id ?? ''),
            Name: String(rawProduct.Brand?.Name ?? rawProduct.brand?.name ?? ''),
            Logo: (rawProduct.Brand?.Logo ?? rawProduct.brand?.logo ?? null) as string | null,
          },
          Category: {
            Id: String(rawProduct.Category?.Id ?? rawProduct.category?.id ?? ''),
            Name: String(rawProduct.Category?.Name ?? rawProduct.category?.name ?? ''),
          },
          Attributes: getSafeArray(rawProduct.Attributes ?? rawProduct.attributes).map((attr) => ({
            AttributeId: String(attr.AttributeId ?? attr.attributeId ?? ''),
            AttributeName: String(attr.AttributeName ?? attr.attributeName ?? ''),
            DisplayOrder: toNumber(attr.DisplayOrder ?? attr.displayOrder ?? 0),
            HasVariant: Boolean(attr.HasVariant ?? attr.hasVariant ?? false),
          })),
          Variants: getSafeArray(rawProduct.Variants ?? rawProduct.variants).map((variant) => ({
            Id: String(variant.Id ?? variant.id ?? ''),
            Name: String(variant.Name ?? variant.name ?? ''),
            Sku: String(variant.Sku ?? variant.sku ?? ''),
            OverrideCost: normalizeMoney(variant.OverrideCost ?? variant.overrideCost ?? null),
            OverridePrice: normalizeMoney(variant.OverridePrice ?? variant.overridePrice ?? null),
            Dimensions: normalizeDimensions(variant.Dimensions ?? variant.dimensions ?? null),
            MainImage: (variant.MainImage ?? variant.mainImage ?? null) as string | null,
            Images: getSafeArray(variant.Images ?? variant.images).map((img) => String(img)),
            AttributeValues: getSafeArray(variant.AttributeValues ?? variant.attributeValues).map((value) => ({
              ProductAttributeId: String(value.ProductAttributeId ?? value.productAttributeId ?? ''),
              AttributeName: String(value.AttributeName ?? value.attributeName ?? ''),
              ValueId: String(value.ValueId ?? value.valueId ?? ''),
              ValueName: String(value.ValueName ?? value.valueName ?? ''),
              ColorCode: (value.ColorCode ?? value.colorCode ?? null) as string | null,
            })),
          })),
        }

        const mappedQuantities: VariantQuantity[] = getSafeArray(rawVariantQuantities).map((item) => ({
          VariantId: String(item.VariantId ?? item.variantId ?? ''),
          QuantityAvailable: Number(item.QuantityAvailable ?? item.quantityAvailable ?? 0),
        }))

        setProduct(mappedProduct)
        setVariantQuantities(mappedQuantities)
        setSelectedAttributes({})
        setSelectedVariantId(null)
        setSelectedImageIndex(0)
      } catch (fetchError) {
        const err = fetchError as Error
        setError(err.message || 'Có lỗi khi tải dữ liệu.')
      } finally {
        setLoading(false)
      }
    }

    fetchDetails()
  }, [slug])

  const quantityMap = useMemo(() => {
    return new Map(variantQuantities.map((item) => [item.VariantId, item.QuantityAvailable]))
  }, [variantQuantities])

  // Create map from ProductAttributeId to AttributeId by analyzing variants
  const productAttributeIdToAttributeIdMap = useMemo(() => {
    const map = new Map<string, string>()
    if (!product || product.Variants.length === 0 || product.Attributes.length === 0) {
      return map
    }

    // Use first variant to establish mapping by position
    const firstVariant = product.Variants[0]
    if (firstVariant.AttributeValues.length === product.Attributes.length) {
      firstVariant.AttributeValues.forEach((attrValue, index) => {
        if (index < product.Attributes.length) {
          map.set(attrValue.ProductAttributeId, product.Attributes[index].AttributeId)
        }
      })
    }
    return map
  }, [product])

  // Create map from AttributeId to AttributeName
  const attributeIdToNameMap = useMemo(() => {
    const map = new Map<string, string>()
    if (!product) return map
    product.Attributes.forEach((attr) => {
      map.set(attr.AttributeId, attr.AttributeName)
    })
    return map
  }, [product])

  const attributeGroups = useMemo(() => {
    if (!product) return []

    // Group variant values by AttributeId (resolved from ProductAttributeId)
    const valueMap = new Map<string, Map<string, VariantAttributeValue>>()

    product.Variants.forEach((variant) => {
      variant.AttributeValues.forEach((value) => {
        // Resolve AttributeId from ProductAttributeId
        const attributeId = productAttributeIdToAttributeIdMap.get(value.ProductAttributeId) || ''
        const productAttr = product.Attributes.find(attr => attr.AttributeId === attributeId)
        const attributeName = value.AttributeName || productAttr?.AttributeName || 'Thuộc tính'

        // Use AttributeId as key to ensure correct grouping
        const key = attributeId || attributeName
        const attrMap = valueMap.get(key) ?? new Map<string, VariantAttributeValue>()
        if (!attrMap.has(value.ValueId)) {
          attrMap.set(value.ValueId, value)
        }
        valueMap.set(key, attrMap)
      })
    })

    // Build attributes list from product.Attributes (only those with variant values)
    const attributes: Array<{ AttributeId: string; AttributeName: string; DisplayOrder: number; HasVariant: boolean }> = []

    product.Attributes.forEach((attr) => {
      // Check if this attribute has values in variants
      const hasValues = product.Variants.some(variant =>
        variant.AttributeValues.some(value => {
          const mappedAttributeId = productAttributeIdToAttributeIdMap.get(value.ProductAttributeId)
          return mappedAttributeId === attr.AttributeId
        })
      )

      if (hasValues) {
        attributes.push(attr)
      }
    })

    return attributes
      .sort((a, b) => a.DisplayOrder - b.DisplayOrder)
      .map((attr) => ({
        id: attr.AttributeId || attr.AttributeName,
        name: attr.AttributeName,
        values: Array.from(valueMap.get(attr.AttributeId)?.values() ?? []),
      }))
  }, [product, productAttributeIdToAttributeIdMap])

  const matchingVariants = useMemo(() => {
    if (!product) return []

    const selectedEntries = Object.entries(selectedAttributes)
    if (selectedEntries.length === 0) return product.Variants
    return product.Variants.filter((variant) =>
      selectedEntries.every(([attrName, valueId]) =>
        variant.AttributeValues.some((value) => {
          const mappedAttributeId = productAttributeIdToAttributeIdMap.get(value.ProductAttributeId)
          const resolvedAttributeName = value.AttributeName || attributeIdToNameMap.get(mappedAttributeId || '') || ''
          return resolvedAttributeName === attrName && value.ValueId === valueId
        }),
      ),
    )
  }, [product, selectedAttributes, productAttributeIdToAttributeIdMap, attributeIdToNameMap])

  const selectedVariant = useMemo(() => {
    if (!product) return null

    // If variant is explicitly selected by clicking variant tag, use it
    if (selectedVariantId) {
      const explicitVariant = product.Variants.find(v => v.Id === selectedVariantId)
      if (explicitVariant) return explicitVariant
    }

    // Otherwise, use attribute-based selection
    if (attributeGroups.length === 0) return product.Variants[0] ?? null
    if (Object.keys(selectedAttributes).length < attributeGroups.length) return null
    return matchingVariants[0] ?? null
  }, [product, attributeGroups, selectedAttributes, matchingVariants, selectedVariantId])

  const selectedQuantity = selectedVariant
    ? quantityMap.get(selectedVariant.Id)
    : undefined
  const isPurchasable =
    !!selectedVariant && (selectedQuantity === undefined || selectedQuantity > 0)

  const displayedImages = useMemo(() => {
    if (!product) return []
    const productImages = uniqueImages([product.Thumbnail, ...product.Images])
    const allVariantImages = uniqueImages(
      product.Variants.flatMap((variant) => [variant.MainImage, ...variant.Images]),
    )
    if (selectedVariant) {
      return uniqueImages([
        selectedVariant.MainImage,
        ...selectedVariant.Images,
        ...productImages,
      ])
    }
    return uniqueImages([...productImages, ...allVariantImages])
  }, [product, selectedVariant])

  useEffect(() => {
    if (selectedVariant && selectedVariant.MainImage) {
      const mainImageIndex = displayedImages.findIndex((img) => img === selectedVariant.MainImage)
      if (mainImageIndex >= 0) {
        setSelectedImageIndex(mainImageIndex)
      } else {
        setSelectedImageIndex(0)
      }
    } else {
      setSelectedImageIndex(0)
    }
  }, [selectedVariant?.Id, selectedVariant?.MainImage, displayedImages])

  const currentImage = displayedImages[selectedImageIndex] ?? ''

  const handleSelectAttribute = (attrName: string, valueId: string) => {
    setSelectedAttributes((prev) => {
      const next = { ...prev, [attrName]: valueId }
      return next
    })
    // Clear explicit variant selection when selecting attributes manually
    setSelectedVariantId(null)
  }

  const handleMouseMove = (event: MouseEvent<HTMLDivElement>) => {
    if (!imageRef.current) return
    const rect = imageRef.current.getBoundingClientRect()
    const x = event.clientX - rect.left
    const y = event.clientY - rect.top
    const halfLens = lensSize / 2
    const clampedX = Math.min(rect.width - halfLens, Math.max(halfLens, x))
    const clampedY = Math.min(rect.height - halfLens, Math.max(halfLens, y))
    const bgWidth = rect.width * ZOOM_LEVEL
    const bgHeight = rect.height * ZOOM_LEVEL
    const bgX = -(clampedX * ZOOM_LEVEL - halfLens)
    const bgY = -(clampedY * ZOOM_LEVEL - halfLens)

    setHoverLens({
      visible: true,
      left: clampedX,
      top: clampedY,
      bgX,
      bgY,
      bgWidth,
      bgHeight,
    })
  }

  const handleMouseLeave = () => {
    setHoverLens((prev) => ({ ...prev, visible: false }))
  }

  const basePrice = selectedVariant?.OverridePrice ?? product?.Price
  const originalPrice = product?.Price
  const showDiscount =
    !!basePrice &&
    !!originalPrice &&
    Number.isFinite(basePrice.Amount) &&
    Number.isFinite(originalPrice.Amount) &&
    basePrice.Amount < originalPrice.Amount
  const discountPercent = showDiscount
    ? Math.round(100 - (basePrice!.Amount / originalPrice!.Amount) * 100)
    : 0

  const selectedDimensions =
    selectedVariant?.Dimensions ?? product?.Dimensions ?? null

  const specAttributes = useMemo(() => {
    if (!product) return []

    // Group variant values by AttributeId (resolved from ProductAttributeId)
    const valueMap = new Map<string, Map<string, string>>()

    product.Variants.forEach((variant) => {
      variant.AttributeValues.forEach((value) => {
        // Resolve AttributeId from ProductAttributeId
        const attributeId = productAttributeIdToAttributeIdMap.get(value.ProductAttributeId) || ''
        const productAttr = product.Attributes.find(attr => attr.AttributeId === attributeId)
        const attributeName = value.AttributeName || productAttr?.AttributeName || 'Thuộc tính'

        // Use AttributeId as key to ensure correct grouping
        const key = attributeId || attributeName
        const attrMap = valueMap.get(key) ?? new Map<string, string>()
        if (!attrMap.has(value.ValueId)) {
          attrMap.set(value.ValueId, value.ValueName)
        }
        valueMap.set(key, attrMap)
      })
    })

    // Build attributes list from product.Attributes (only those with variant values)
    const attributes: Array<{ AttributeId: string; AttributeName: string; DisplayOrder: number; HasVariant: boolean }> = []

    product.Attributes.forEach((attr) => {
      // Check if this attribute has values in variants
      const hasValues = product.Variants.some(variant =>
        variant.AttributeValues.some(value => {
          const mappedAttributeId = productAttributeIdToAttributeIdMap.get(value.ProductAttributeId)
          return mappedAttributeId === attr.AttributeId
        })
      )

      if (hasValues) {
        attributes.push(attr)
      }
    })

    return attributes
      .sort((a, b) => a.DisplayOrder - b.DisplayOrder)
      .map((attr) => ({
        id: attr.AttributeId || attr.AttributeName,
        name: attr.AttributeName,
        values: Array.from(valueMap.get(attr.AttributeId)?.values() ?? []),
      }))
  }, [product, productAttributeIdToAttributeIdMap])

  const variantOptions = useMemo(() => {
    if (!product) return []
    return product.Variants.map((variant) => {
      const baseLabel =
        variant.Name ||
        variant.AttributeValues.map((value) => value.ValueName).join(' / ') ||
        variant.Sku
      const quantity = quantityMap.get(variant.Id)
      const label = quantity !== undefined ? `${baseLabel} (${quantity})` : baseLabel
      return {
        id: variant.Id,
        label,
        variant,
      }
    })
  }, [product, quantityMap])

  const handleAddToCart = async () => {
    if (!selectedVariant) {
      showToast('Thiếu biến thể', 'Vui lòng chọn biến thể trước khi thêm vào giỏ.', 'warning')
      return
    }

    setIsAddingToCart(true)
    try {
      await addItem(selectedVariant.Sku, 1)
      showToast('Đã thêm vào giỏ', `${product?.Name ?? 'Sản phẩm'} đã được thêm vào giỏ hàng.`, 'success')
    } catch {
      showToast('Thêm vào giỏ thất bại', 'Vui lòng thử lại sau.', 'error')
    } finally {
      setIsAddingToCart(false)
    }
  }

  const handleBuyNow = async () => {
    if (!selectedVariant) {
      showToast('Thiếu biến thể', 'Vui lòng chọn biến thể trước khi mua.', 'warning')
      return
    }

    setIsAddingToCart(true)
    try {
      await addItem(selectedVariant.Sku, 1)
      navigate('/checkout')
    } catch {
      showToast('Mua ngay thất bại', 'Vui lòng thử lại sau.', 'error')
    } finally {
      setIsAddingToCart(false)
    }
  }

  return (
    <section className="product-details">
      <nav className="breadcrumb product-details__breadcrumb">
        <Link to="/">Trang chủ</Link> / <Link to="/products">Sản phẩm</Link> /{' '}
        <span>{product?.Name ?? slug}</span>
      </nav>

      {error && <div className="empty-state">{error}</div>}
      {!error && loading && <div className="empty-state">Đang tải chi tiết sản phẩm...</div>}
      {!error && !loading && !product && (
        <div className="empty-state">Không tìm thấy sản phẩm.</div>
      )}

      {!error && !loading && product && (
        <>
          <div className="product-details__main">
            <div className="product-details__media">
              <div
                className="product-media-zoom"
                ref={imageRef}
                onMouseMove={handleMouseMove}
                onMouseLeave={handleMouseLeave}
              >
                {currentImage ? (
                  <>
                    <img src={currentImage} alt={product.Name} />
                    <div
                      className={`zoom-lens ${hoverLens.visible ? 'active' : ''}`}
                      style={{
                        left: `${hoverLens.left}px`,
                        top: `${hoverLens.top}px`,
                        backgroundImage: `url(${currentImage})`,
                        backgroundPosition: `${hoverLens.bgX}px ${hoverLens.bgY}px`,
                        backgroundSize: `${hoverLens.bgWidth}px ${hoverLens.bgHeight}px`,
                        width: `${lensSize}px`,
                        height: `${lensSize}px`,
                      }}
                    />
                  </>
                ) : (
                  <div className="empty-state">Chưa có hình ảnh</div>
                )}
              </div>

              {displayedImages.length > 1 && (
                <div className="product-thumbnails">
                  {displayedImages.map((image, index) => (
                    <button
                      key={`${image}-${index}`}
                      type="button"
                      className={`thumbnail ${index === selectedImageIndex ? 'active' : ''}`}
                      onClick={() => setSelectedImageIndex(index)}
                    >
                      <img src={image} alt={`${product.Name} ${index + 1}`} />
                    </button>
                  ))}
                </div>
              )}
            </div>

            <div className="product-details__info">
              <div className="product-brand">{product.Brand?.Name}</div>
              <h1 className="product-title">{product.Name}</h1>
              <p className="product-short-desc">
                {product.Description || 'Thông tin đang được cập nhật.'}
              </p>

              <div className="price">
                <div className="price-row">
                  <span>{formatMoney(basePrice)}</span>
                  {showDiscount && (
                    <span className="discount-tag">-{discountPercent}%</span>
                  )}
                </div>
                {showDiscount && (
                  <small className="original">{formatMoney(originalPrice)}</small>
                )}
              </div>

              <div className="variant-groups">
                {attributeGroups.map((group) => (
                  <div className="variant-group" key={group.name}>
                    <span className="variant-label">{group.name}</span>
                    <div className="variant-tags inline">
                      {group.values.map((value) => {
                        const isSelected = selectedAttributes[group.name] === value.ValueId
                        const otherSelections = Object.entries(selectedAttributes).filter(
                          ([attrName]) => attrName !== group.name,
                        )

                        const hasMatch = product.Variants.some((variant) => {
                          const matchesOther = otherSelections.every(([attrName, valueId]) =>
                            variant.AttributeValues.some((attrValue) => {
                              const mappedAttributeId = productAttributeIdToAttributeIdMap.get(attrValue.ProductAttributeId)
                              const resolvedAttributeName = attrValue.AttributeName || attributeIdToNameMap.get(mappedAttributeId || '') || ''
                              return resolvedAttributeName === attrName && attrValue.ValueId === valueId
                            }),
                          )
                          if (!matchesOther) return false
                          return variant.AttributeValues.some((attrValue) => {
                            const mappedAttributeId = productAttributeIdToAttributeIdMap.get(attrValue.ProductAttributeId)
                            const resolvedAttributeName = attrValue.AttributeName || attributeIdToNameMap.get(mappedAttributeId || '') || ''
                            return resolvedAttributeName === group.name && attrValue.ValueId === value.ValueId
                          })
                        })

                        // Check if this is a color attribute (case-insensitive)
                        const isColorAttribute = group.name.toLowerCase() === 'color' || group.name.toLowerCase() === 'màu sắc'

                        if (isColorAttribute) {
                          const colorValue = value.ColorCode ? normalizeColor(value.ColorCode) : normalizeColor(value.ValueName)
                          return (
                            <button
                              key={value.ValueId}
                              type="button"
                              className={`color-dot ${isSelected ? 'active' : ''}`}
                              onClick={() => handleSelectAttribute(group.name, value.ValueId)}
                              disabled={!hasMatch}
                              title={value.ValueName}
                              style={{
                                backgroundColor: colorValue,
                                opacity: !hasMatch ? 0.3 : 1,
                                cursor: !hasMatch ? 'not-allowed' : 'pointer',
                              }}
                            />
                          )
                        }

                        return (
                          <button
                            key={value.ValueId}
                            type="button"
                            className={`variant-tag ${isSelected ? 'active' : ''}`}
                            onClick={() => handleSelectAttribute(group.name, value.ValueId)}
                            disabled={!hasMatch}
                            style={{
                              borderColor: value.ColorCode ?? undefined,
                            }}
                          >
                            {value.ValueName}
                          </button>
                        )
                      })}
                    </div>
                  </div>
                ))}
              </div>

              {variantOptions.length > 0 && (
                <div className="variant-group">
                  <span className="variant-label">Biến thể</span>
                  <div className="variant-tags inline">
                    {variantOptions.map((option) => (
                      <button
                        key={option.id}
                        type="button"
                        className={`variant-tag ${selectedVariant?.Id === option.id ? 'active' : ''}`}
                        onClick={() => {
                          // Set explicit variant selection
                          setSelectedVariantId(option.id)

                          // Also update selectedAttributes for consistency
                          const nextSelected: Record<string, string> = {}
                          option.variant.AttributeValues.forEach((value) => {
                            const mappedAttributeId = productAttributeIdToAttributeIdMap.get(value.ProductAttributeId)
                            const attributeName = value.AttributeName || attributeIdToNameMap.get(mappedAttributeId || '')
                            if (attributeName) {
                              nextSelected[attributeName] = value.ValueId
                            }
                          })
                          setSelectedAttributes(nextSelected)

                          // Jump to variant's main image
                          if (option.variant.MainImage) {
                            const mainImageIndex = displayedImages.findIndex((img) => img === option.variant.MainImage)
                            if (mainImageIndex >= 0) {
                              setSelectedImageIndex(mainImageIndex)
                            } else {
                              setSelectedImageIndex(0)
                            }
                          } else {
                            setSelectedImageIndex(0)
                          }
                        }}
                      >
                        {option.label}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              <div
                ref={actionsWrapperRef}
                className="product-actions-wrapper"
                style={actionsHeight ? { minHeight: `${actionsHeight}px` } : undefined}
              >
                <div
                  ref={actionsRef}
                  className={`product-actions${stickyMode === 'top' ? ' is-fixed-top' : ''}${stickyMode === 'bottom' ? ' is-fixed-bottom' : ''
                    }`}
                  style={
                    stickyMode === 'normal'
                      ? undefined
                      : {
                        width: `${stickyStyle.width}px`,
                        left: `${stickyStyle.left}px`,
                      }
                  }
                >
                  <button
                    className="secondary"
                    type="button"
                    disabled={!isPurchasable || isAddingToCart}
                    onClick={() => { void handleAddToCart() }}
                  >
                    Thêm vào giỏ
                  </button>
                  <button
                    className="primary"
                    type="button"
                    disabled={!isPurchasable || isAddingToCart}
                    onClick={() => { void handleBuyNow() }}
                  >
                    Mua ngay
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div className="product-details__description">
            <h2>Mô tả sản phẩm</h2>
            <p>{product.Description || 'Thông tin đang được cập nhật.'}</p>

            <div className="product-details__specs">
              <h3>Thông số</h3>
              <table>
                <tbody>
                  <tr>
                    <th>Thương hiệu</th>
                    <td>{product.Brand?.Name || '--'}</td>
                  </tr>
                  <tr>
                    <th>Danh mục</th>
                    <td>{product.Category?.Name || '--'}</td>
                  </tr>
                  {selectedDimensions && (
                    <tr>
                      <th>Kích thước</th>
                      <td>
                        {selectedDimensions.Width} x {selectedDimensions.Height} x{' '}
                        {selectedDimensions.Depth} cm
                      </td>
                    </tr>
                  )}
                  {selectedDimensions && (
                    <tr>
                      <th>Trọng lượng</th>
                      <td>{selectedDimensions.Weight} kg</td>
                    </tr>
                  )}
                  {specAttributes.map((attr) => (
                    <tr key={attr.id}>
                      <th>{attr.name}</th>
                      <td>{attr.values.length > 0 ? attr.values.join(', ') : '--'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </section>
  )
}

export default ProductDetails
