type SelectOption = {
  value: string
  label: string
}

type SelectFieldProps = {
  label: string
  placeholder?: string
  options: SelectOption[]
}

function SelectField({ label, placeholder, options }: SelectFieldProps) {
  return (
    <label className="custom-select">
      {label}
      <select>
        {placeholder && <option value="">{placeholder}</option>}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  )
}

export default SelectField
