using AutoMapper;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IMapper _mapper;

        public FornecedoresController(
            IFornecedorRepository fornecedorRepository,
            IMapper mapper,
            IFornecedorService fornecedorService,
            INotificador notificador,
            IEnderecoRepository enderecoRepository = null
        ) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _enderecoRepository = enderecoRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<FornecedorDto>> ObterTodos()
        {
            var fornecedor = _mapper.Map<IEnumerable<FornecedorDto>>(await _fornecedorRepository.ObterTodos());

            return fornecedor;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> ObterPorId(Guid id)
        {
            var fornecedor = await ObterFornecedorProdutosEndereco(id);

            if (fornecedor == null) return NotFound();

            return fornecedor;
        }

        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> Adicionar(FornecedorDto fornecedorViewModel)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var result = await _fornecedorService.Adicionar(
                _mapper.Map<Fornecedor>(fornecedorViewModel)
            );

            return CustomResponse(fornecedorViewModel);
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Atualizar(
            Guid id,
            FornecedorDto fornecedorViewModel
        )
        {
            if (id != fornecedorViewModel.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(fornecedorViewModel);
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);

            await _fornecedorService.Atualizar(fornecedor);

            return CustomResponse(fornecedorViewModel);
        }

        [ClaimsAuthorize("Fornecedor", "Excluir")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Excluir(Guid id)
        {
            var fornecedorViewModel = ObterFornecedorEndereco(id);

            if (fornecedorViewModel == null) return NotFound();

            await _fornecedorService.Remover(id);

            return CustomResponse();
        }

        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<EnderecoDto> ObterEnderecoPorId(Guid id)
        {
            return _mapper.Map<EnderecoDto>(await _enderecoRepository.ObterPorId(id));
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> AtualizarEndereco(
            Guid id,
            EnderecoDto enderecoViewModel
        )
        {
            if (id != enderecoViewModel.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(enderecoViewModel);
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            await _fornecedorService.AtualizarEndereco(_mapper.Map<Endereco>(enderecoViewModel));

            return CustomResponse(enderecoViewModel);
        }

        private async Task<FornecedorDto> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        private async Task<FornecedorDto> ObterFornecedorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}
