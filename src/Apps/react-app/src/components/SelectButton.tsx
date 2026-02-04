import { type ComponentProps } from 'react'

type SelectButtonProps = {
    label: string
    isActive?: boolean
    onClick?: () => void
    variant?: 'solid' | 'outline'
    type?: 'text' | 'color'
    colorCode?: string
} & Omit<ComponentProps<'button'>, 'title' | 'type' | 'onClick'>

function SelectButton({
    label,
    isActive = false,
    onClick,
    variant = 'outline',
    type = 'text',
    colorCode,
    className = '',
    ...props
}: SelectButtonProps) {

    if (type === 'color' && colorCode) {
        return (
            <button
                type="button"
                className={`color-swatch ${isActive ? 'active' : ''} ${className}`}
                onClick={onClick}
                title={label}
                style={{ backgroundColor: colorCode }}
                {...props}
            >
                {isActive && (
                    <span className="color-swatch-check" style={{ color: label === 'White' ? '#000' : '#fff' }}>
                        ✓
                    </span>
                )}
            </button>
        )
    }

    return (
        <button
            type="button"
            className={`filter-chip ${variant} ${isActive ? 'active' : ''} ${className}`}
            onClick={onClick}
            {...props}
        >
            {label}
        </button>
    )
}

export default SelectButton
