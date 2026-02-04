import { type ReactNode, useState } from 'react'
import * as Dialog from '@radix-ui/react-dialog'
import SelectButton from './SelectButton'


export type FilterValues = {
    brand: string[]
    category: string
    minPrice: string
    maxPrice: string
    colors: string[]
}

type FilterModalProps = {
    trigger: ReactNode
    initialFilters?: FilterValues
    onApply: (filters: FilterValues) => void
}

const BRANDS = ['Nike', 'Adidas', 'Puma', 'Reebok', 'New Balance']
const CATEGORIES = ['All', 'Shoes', 'Apparel', 'Accessories', 'Electronics']
const COLORS = [
    { name: 'Red', code: '#ef4444' },
    { name: 'Blue', code: '#3b82f6' },
    { name: 'Green', code: '#22c55e' },
    { name: 'Black', code: '#000000' },
    { name: 'White', code: '#ffffff' },
]

function FilterModal({ trigger, initialFilters, onApply }: FilterModalProps) {
    const [filters, setFilters] = useState<FilterValues>(initialFilters || {
        brand: [],
        category: 'All',
        minPrice: '',
        maxPrice: '',
        colors: []
    })
    const [isOpen, setIsOpen] = useState(false)

    const handleApply = () => {
        onApply(filters)
        setIsOpen(false)
    }

    const toggleBrand = (brand: string) => {
        setFilters(prev => ({
            ...prev,
            brand: prev.brand.includes(brand)
                ? prev.brand.filter(b => b !== brand)
                : [...prev.brand, brand]
        }))
    }

    const toggleColor = (color: string) => {
        setFilters(prev => ({
            ...prev,
            colors: prev.colors.includes(color)
                ? prev.colors.filter(c => c !== color)
                : [...prev.colors, color]
        }))
    }

    return (
        <Dialog.Root open={isOpen} onOpenChange={setIsOpen}>
            <Dialog.Trigger asChild>{trigger}</Dialog.Trigger>
            <Dialog.Portal>
                <Dialog.Overlay className="radix-overlay" />
                <Dialog.Content className="radix-modal" style={{ width: '480px', maxWidth: '95vw', background: 'var(--noel-card-bg, rgba(7, 12, 22, 0.98))' }}>
                    <Dialog.Close className="modal-close-btn icon-button" aria-label="Close">
                        ✕
                    </Dialog.Close>

                    <div className="modal-header">
                        <Dialog.Title className="modal-title">Bộ lọc tìm kiếm</Dialog.Title>
                    </div>

                    <div style={{ padding: '20px 0', display: 'flex', flexDirection: 'column', gap: '24px' }}>
                        {/* Category */}
                        <div>
                            <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600, fontSize: '0.9rem' }}>Danh mục</label>
                            <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                {CATEGORIES.map(cat => (
                                    <SelectButton
                                        key={cat}
                                        label={cat}
                                        isActive={filters.category === cat}
                                        onClick={() => setFilters(prev => ({ ...prev, category: cat }))}
                                        variant="solid"
                                    />
                                ))}
                            </div>
                        </div>

                        {/* Brand */}
                        <div>
                            <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600, fontSize: '0.9rem' }}>Thương hiệu</label>
                            <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                                {BRANDS.map(brand => (
                                    <SelectButton
                                        key={brand}
                                        label={brand}
                                        isActive={filters.brand.includes(brand)}
                                        onClick={() => toggleBrand(brand)}
                                        variant="outline"
                                    />
                                ))}
                            </div>
                        </div>

                        {/* Price Range */}
                        <div>
                            <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600, fontSize: '0.9rem' }}>Khoảng giá</label>
                            <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
                                <input
                                    type="number"
                                    placeholder="Min"
                                    value={filters.minPrice}
                                    onChange={e => setFilters(prev => ({ ...prev, minPrice: e.target.value }))}
                                    className="filter-input"
                                    style={{ flex: 1 }}
                                />
                                <span>-</span>
                                <input
                                    type="number"
                                    placeholder="Max"
                                    value={filters.maxPrice}
                                    onChange={e => setFilters(prev => ({ ...prev, maxPrice: e.target.value }))}
                                    className="filter-input"
                                    style={{ flex: 1 }}
                                />
                            </div>
                        </div>

                        {/* Colors */}
                        <div>
                            <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600, fontSize: '0.9rem' }}>Màu sắc</label>
                            <div style={{ display: 'flex', gap: '12px' }}>
                                {COLORS.map(c => (
                                    <SelectButton
                                        key={c.name}
                                        label={c.name}
                                        type="color"
                                        colorCode={c.code}
                                        isActive={filters.colors.includes(c.name)}
                                        onClick={() => toggleColor(c.name)}
                                    />
                                ))}
                            </div>
                        </div>

                    </div>

                    <div style={{ display: 'flex', gap: '12px', marginTop: '10px' }}>
                        <button
                            className="btn"
                            onClick={() => setFilters({ brand: [], category: 'All', minPrice: '', maxPrice: '', colors: [] })}
                            style={{ flex: 1, background: 'rgba(255,255,255,0.1)', color: 'var(--text)', border: 'none', padding: '12px', borderRadius: '12px', cursor: 'pointer' }}
                        >
                            Đặt lại
                        </button>
                        <button
                            className="btn btn-primary"
                            onClick={handleApply}
                            style={{ flex: 1, padding: '12px', borderRadius: '12px', cursor: 'pointer', border: 'none', fontWeight: 600 }}
                        >
                            Áp dụng
                        </button>
                    </div>

                </Dialog.Content>
            </Dialog.Portal>
        </Dialog.Root>
    )
}

export default FilterModal
