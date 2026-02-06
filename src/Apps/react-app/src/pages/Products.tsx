import { useEffect, useState } from 'react'
import { api, API_PREFIX } from '../config/api'
import ProductCard, { type ProductCardData } from '../components/ProductCard'
import RadixSelect from '../components/RadixSelect'
import FilterModal, { type FilterValues } from '../components/FilterModal'
import Pagination from '../components/Pagination'

type PaginatedResult<T> = {
  PageNumber: number
  PageSize: number
  TotalItems: number
  TotalPages: number
  HasNext: boolean
  HasPrevious: boolean
  Items: T[]
}

type ProductCardDto = {
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
  VariantDots?: Array<{
    AttributeName: string
    DisplayType: string
    ValueStyleCss?: string | null
    Values: Array<{
      Id: string
      Value: string
      ColorCode?: string | null
    }>
  }>
  Status: string
}

const PLACEHOLDER_IMAGES = [
  'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1503602642458-232111445657?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1512436991641-6745cdb1723f?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1524592094714-0f0654e20314?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1514986888952-8cd320577b68?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1503341455253-b2e723bb3dbb?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=800&q=80',
  'https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=800&q=80',
]

const hashString = (value: string) =>
  value.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0)

const getImageForProduct = (id: string) => {
  const index = Math.abs(hashString(id)) % PLACEHOLDER_IMAGES.length
  return PLACEHOLDER_IMAGES[index]
}

function Products() {
  const [productsResult, setProductsResult] = useState<PaginatedResult<ProductCardDto> | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [keyword, setKeyword] = useState('')
  const [debouncedKeyword, setDebouncedKeyword] = useState('')
  const [sortBy, setSortBy] = useState('featured')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')
  const [page, setPage] = useState(1)
  const [filters, setFilters] = useState<FilterValues>({
    brand: [],
    category: 'All',
    minPrice: '',
    maxPrice: '',
    colors: []
  })

  const pageSize = 12

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedKeyword(keyword.trim()), 350)
    return () => clearTimeout(timer)
  }, [keyword])

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true)
      setError(null)
      try {
          const response = await api.get(`${API_PREFIX}/products`, {
          params: {
            page,
            pageSize,
            keyword: debouncedKeyword || undefined,
            sortColumn: sortBy,
            sortOrder,
            brand: filters.brand.join(',') || undefined,
            category: filters.category === 'All' ? undefined : filters.category,
            minPrice: filters.minPrice || undefined,
            maxPrice: filters.maxPrice || undefined,
            colors: filters.colors.join(',') || undefined,
          },
        })

        const raw = response.data
        const rawItems = (raw.Items ?? raw.items ?? []) as Record<string, unknown>[]
        const mappedItems: ProductCardDto[] = rawItems.map((item) => {
          const rawVariantDots = (item.VariantDots ?? item.variantDots ?? []) as Record<string, unknown>[]
          const variantDots = rawVariantDots.map((dot) => {
            const rawValues = (dot.Values ?? dot.values ?? []) as Record<string, unknown>[]
            return {
              AttributeName: String(dot.AttributeName ?? dot.attributeName ?? ''),
              DisplayType: String(dot.DisplayType ?? dot.displayType ?? ''),
              ValueStyleCss: (dot.ValueStyleCss ?? dot.valueStyleCss ?? null) as string | null,
              Values: rawValues.map((value) => ({
                Id: String(value.Id ?? value.id ?? ''),
                Value: String(value.Value ?? value.value ?? ''),
                ColorCode: (value.ColorCode ?? value.colorCode ?? null) as string | null,
              })),
            }
          })

          return {
            Id: String(item.Id ?? item.id ?? ''),
            Name: String(item.Name ?? item.name ?? ''),
            Description: (item.Description ?? item.description ?? null) as string | null,
            Slug: String(item.Slug ?? item.slug ?? ''),
            OriginalPrice: Number(item.OriginalPrice ?? item.originalPrice ?? 0),
            DiscountPercent: Number(item.DiscountPercent ?? item.discountPercent ?? 0),
            DiscountedPrice: Number(item.DiscountedPrice ?? item.discountedPrice ?? 0),
            Currency: String(item.Currency ?? item.currency ?? 'VND'),
            Rating: Number(item.Rating ?? item.rating ?? 0),
            SoldCount: Number(item.SoldCount ?? item.soldCount ?? 0),
            FeedbackCount: Number(item.FeedbackCount ?? item.feedbackCount ?? 0),
            FeaturedTag: String(item.FeaturedTag ?? item.featuredTag ?? 'None'),
            Thumbnail: (item.Thumbnail ?? item.thumbnail ?? null) as string | null,
            SecondaryImage: (item.SecondaryImage ?? item.secondaryImage ?? null) as string | null,
            VariantDots: variantDots,
            Status: String(item.Status ?? item.status ?? ''),
          }
        })

        const data: PaginatedResult<ProductCardDto> = {
          PageNumber: Number(raw.PageNumber ?? raw.pageNumber ?? raw.page ?? 1),
          PageSize: Number(raw.PageSize ?? raw.pageSize ?? pageSize),
          TotalItems: Number(raw.TotalItems ?? raw.totalItems ?? mappedItems.length),
          TotalPages: Number(raw.TotalPages ?? raw.totalPages ?? 1),
          HasNext: Boolean(raw.HasNext ?? raw.hasNext ?? false),
          HasPrevious: Boolean(raw.HasPrevious ?? raw.hasPrevious ?? false),
          Items: mappedItems,
        }

        setProductsResult(data)
      } catch (fetchError) {
          const err = fetchError as Error;
          setError(err.message || 'Có lỗi khi tải dữ liệu.');
      } finally {
        setLoading(false)
      }
    }
    fetchProducts()
  }, [page, pageSize, debouncedKeyword, sortBy, sortOrder, filters])

  const products = productsResult?.Items ?? []
  const totalPages = productsResult?.TotalPages ?? 1

  useEffect(() => {
    if (page > totalPages && totalPages > 0) {
      setPage(1)
    }
  }, [page, totalPages])

  const handleSortChange = (value: string) => {
    const [nextSort, nextOrder] = value.split(':')
    setSortBy(nextSort)
    setSortOrder((nextOrder as 'asc' | 'desc') || 'asc')
    setPage(1)
  }

  const handleApplyFilters = (newFilters: FilterValues) => {
    setFilters(newFilters)
    setPage(1)
  }

  return (
    <>
      <section>
        <div className="filters">
          <div className="filter-left">
            <div className="search">
              <span className="search-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="11" cy="11" r="8" />
                  <path d="m21 21-4.3-4.3" />
                </svg>
              </span>
              <input
                type="text"
                placeholder="Tìm kiếm sản phẩm..."
                value={keyword}
                onChange={(event) => {
                  setKeyword(event.target.value)
                  setPage(1)
                }}
              />
            </div>
            <FilterModal
              initialFilters={filters}
              onApply={handleApplyFilters}
              trigger={(
                <button className="icon-button" style={{ width: '48px', height: '48px', borderRadius: '50%', border: '1px solid var(--glass-border)', background: 'var(--noel-search-bg, rgba(5,11,20,0.75))', color: 'var(--text)', cursor: 'pointer', display: 'grid', placeItems: 'center', transition: 'all 0.2s' }} aria-label="Bộ lọc">
                  <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <line x1="4" x2="4" y1="21" y2="14" />
                    <line x1="4" x2="4" y1="10" y2="3" />
                    <line x1="12" x2="12" y1="21" y2="12" />
                    <line x1="12" x2="12" y1="8" y2="3" />
                    <line x1="20" x2="20" y1="21" y2="16" />
                    <line x1="20" x2="20" y1="12" y2="3" />
                    <line x1="1" x2="7" y1="14" y2="14" />
                    <line x1="9" x2="15" y1="8" y2="8" />
                    <line x1="17" x2="23" y1="16" y2="16" />
                  </svg>
                </button>
              )}
            />
          </div>

          <div className="filter-center" style={{ display: 'flex', justifyContent: 'center' }}>
            <Pagination
              currentPage={page}
              totalPages={totalPages}
              onPageChange={setPage}
            />
          </div>

          <div className="filter-right">
            <RadixSelect
              value={`${sortBy}:${sortOrder}`}
              onChange={handleSortChange}
              placeholder="Sắp xếp"
              options={[
                { value: 'featured:desc', label: 'Nổi bật' },
                { value: 'price:asc', label: 'Giá tăng dần' },
                { value: 'price:desc', label: 'Giá giảm dần' },
                { value: 'discount:desc', label: 'Giảm nhiều' },
                { value: 'rating:desc', label: 'Đánh giá cao' },
                { value: 'name:asc', label: 'Tên A → Z' },
              ]}
            />
          </div>
        </div>
      </section>

      <section id="products">
        {/* <p className="section-heading">Tuyển chọn cá nhân hóa</p> */}
        {error && <div className="empty-state">{error}</div>}
        {!error && loading && <div className="empty-state">Đang tải sản phẩm...</div>}
        {!error && !loading && (
          <>
            <div className="products-grid">
              {products.map((product) => {
                const cardData: ProductCardData = {
                  Id: product.Id,
                  Name: product.Name,
                  Description: product.Description,
                  Slug: product.Slug,
                  OriginalPrice: product.OriginalPrice ?? 0,
                  DiscountPercent: product.DiscountPercent ?? 0,
                  DiscountedPrice: product.DiscountedPrice ?? 0,
                  Currency: product.Currency ?? 'VND',
                  Rating: product.Rating ?? 0,
                  SoldCount: product.SoldCount ?? 0,
                  FeedbackCount: product.FeedbackCount ?? 0,
                  FeaturedTag: product.FeaturedTag ?? 'None',
                  Thumbnail: product.Thumbnail,
                  SecondaryImage: product.SecondaryImage,
                  VariantDots: product.VariantDots ?? [],
                }
                const imageUrl = product.Thumbnail || getImageForProduct(product.Id)

                return (
                  <ProductCard
                    key={product.Id}
                    product={cardData}
                    imageUrl={imageUrl}
                  />
                )
              })}
            </div>
            {products.length === 0 && (
              <div className="empty-state">Không tìm thấy sản phẩm phù hợp.</div>
            )}

            {/* Bottom Pagination */}
            <div style={{ marginTop: '60px' }}>
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                onPageChange={setPage}
              />
            </div>
          </>
        )}
      </section>
    </>
  )
}

export default Products
