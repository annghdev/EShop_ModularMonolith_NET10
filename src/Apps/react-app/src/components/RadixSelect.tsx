import * as Select from '@radix-ui/react-select'
import { forwardRef } from 'react'

type SelectOption = {
    value: string
    label: string
}

type RadixSelectProps = {
    options: SelectOption[]
    value?: string
    onChange?: (value: string) => void
    placeholder?: string
    label?: string
    className?: string
}

const SelectItem = forwardRef<HTMLDivElement, Select.SelectItemProps>(
    ({ children, ...props }, ref) => (
        <Select.Item className="radix-select-item" {...props} ref={ref}>
            <Select.ItemText>{children}</Select.ItemText>
            <Select.ItemIndicator className="radix-select-indicator">
                ✓
            </Select.ItemIndicator>
        </Select.Item>
    )
)

SelectItem.displayName = 'SelectItem'

function RadixSelect({
    options,
    value,
    onChange,
    placeholder = 'Chọn...',
    label,
    className = ''
}: RadixSelectProps) {
    return (
        <div className={`radix-select-wrapper ${className}`}>
            {label && <span className="radix-select-label">{label}</span>}
            <Select.Root value={value} onValueChange={onChange}>
                <Select.Trigger className="radix-select-trigger">
                    <Select.Value placeholder={placeholder} />
                    <Select.Icon className="radix-select-icon">
                        ▾
                    </Select.Icon>
                </Select.Trigger>
                <Select.Portal>
                    <Select.Content className="radix-select-content" position="popper" sideOffset={6}>
                        <Select.ScrollUpButton className="radix-select-scroll-btn">▴</Select.ScrollUpButton>
                        <Select.Viewport className="radix-select-viewport">
                            {options.map((option) => (
                                <SelectItem key={option.value} value={option.value}>
                                    {option.label}
                                </SelectItem>
                            ))}
                        </Select.Viewport>
                        <Select.ScrollDownButton className="radix-select-scroll-btn">▾</Select.ScrollDownButton>
                    </Select.Content>
                </Select.Portal>
            </Select.Root>
        </div>
    )
}

export default RadixSelect
