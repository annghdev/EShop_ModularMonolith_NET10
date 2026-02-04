
type PaginationProps = {
    currentPage: number
    totalPages: number
    onPageChange: (page: number) => void
}

function Pagination({ currentPage, totalPages, onPageChange }: PaginationProps) {
    if (totalPages <= 1) return null

    // Helper to generate page numbers
    const getPages = () => {
        const pages: number[] = []
        const start = Math.max(1, currentPage - 2)
        const end = Math.min(totalPages, currentPage + 2)
        for (let i = start; i <= end; i += 1) {
            pages.push(i)
        }
        return pages
    }

    const paginationPages = getPages()

    return (
        <div className="pagination" style={{ marginTop: 0 }}>
            {/* Use inline style marginTop 0 to override any default margins when placed in header */}
            <button
                type="button"
                onClick={() => onPageChange(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
                aria-label="Previous Page"
                style={{ width: '36px', height: '36px', fontSize: '1rem' }}
            >
                ‹
            </button>
            {paginationPages.map((pageNumber) => (
                <button
                    key={pageNumber}
                    type="button"
                    className={pageNumber === currentPage ? 'active' : ''}
                    onClick={() => onPageChange(pageNumber)}
                    style={{ width: '36px', height: '36px', fontSize: '0.9rem' }}
                >
                    {pageNumber}
                </button>
            ))}
            <button
                type="button"
                onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
                disabled={currentPage === totalPages}
                aria-label="Next Page"
                style={{ width: '36px', height: '36px', fontSize: '1rem' }}
            >
                ›
            </button>
        </div>
    )
}

export default Pagination
