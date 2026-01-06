namespace BookStoreManagmentSystem.DTO_s
{
    public record struct CreateAuthorDto(string Name, List<CreateBookDto> Books);
}
