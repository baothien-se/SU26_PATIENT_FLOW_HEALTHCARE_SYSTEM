using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProductService.Application.Common.Interfaces;

namespace ProductService.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return false;

        await _productRepository.DeleteAsync(product, cancellationToken);
        return true;
    }
}
