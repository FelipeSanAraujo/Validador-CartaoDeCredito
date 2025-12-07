using System;
using System.Text.RegularExpressions;

namespace ValidadorCartao
{
    public class CartaoCredito
    {
        public string Numero { get; set; }
        public string Bandeira { get; set; }
        public bool IsValido { get; set; }
    }

    public class ValidadorCartao
    {
        private static readonly Dictionary<string, (string bandeira, int[] comprimentos, string padrao)> Bandeiras = 
            new()
            {
                { "Visa", ("Visa", new[] { 12, 16 }, "^4[0-9]{11,15}$") },
                { "MasterCard", ("MasterCard", new[] { 16 }, "^5[1-5][0-9]{14}$|^2[2-7][0-9]{14}$") },
                { "American Express", ("American Express", new[] { 15 }, "^3[47][0-9]{13}$") },
                { "Diners Club", ("Diners Club", new[] { 14 }, "^3(?:0[0-5]|[68][0-9])[0-9]{11}$") },
                { "Discover", ("Discover", new[] { 16 }, "^6(?:011|5[0-9]{2})[0-9]{12}$") },
                { "EnRoute", ("EnRoute", new[] { 15 }, "^2014[0-9]{11}$") },
                { "JCB", ("JCB", new[] { 16 }, "^(?:2131|1800|35\\d{3})\\d{11}$") },
                { "Voyager", ("Voyager", new[] { 15 }, "^8699[0-9]{11}$") },
                { "HiperCard", ("HiperCard", new[] { 16 }, "^(384100|384140|384160|606282|637095|637612|637613|637649|637650|639047|639348|639599|639947|640337|640355|640356|640357|640358|640359|640360|640361|640362|640363|640364|640365|640366|640367|640368|640369)") },
                { "Aura", ("Aura", new[] { 16 }, "^50[0-9]{14}$") }
            };

        /// <summary>
        /// Valida um número de cartão de crédito e identifica a bandeira.
        /// </summary>
        /// <param name="numeroCartao">String contendo o número do cartão (apenas dígitos)</param>
        /// <returns>Objeto CartaoCredito com informações de validação e bandeira</returns>
        public CartaoCredito Validar(string numeroCartao)
        {
            var resultado = new CartaoCredito
            {
                Numero = numeroCartao,
                IsValido = false,
                Bandeira = "Desconhecida"
            };

            if (string.IsNullOrWhiteSpace(numeroCartao))
                return resultado;

            var numeroLimpo = Regex.Replace(numeroCartao, @"[^\d]", "");

            if (!Regex.IsMatch(numeroLimpo, @"^\d+$"))
                return resultado;

            var bandeira = IdentificarBandeira(numeroLimpo);
            if (bandeira == null)
                return resultado;

            resultado.Bandeira = bandeira.Value.bandeira;

            if (!bandeira.Value.comprimentos.Contains(numeroLimpo.Length))
                return resultado;

            resultado.IsValido = ValidarLuhn(numeroLimpo);

            return resultado;
        }

        /// <summary>
        /// Identifica a bandeira do cartão baseado no número.
        /// </summary>
        private (string bandeira, int[] comprimentos, string padrao)? IdentificarBandeira(string numero)
        {
            foreach (var (bandeira, config) in Bandeiras)
            {
                if (Regex.IsMatch(numero, config.padrao))
                    return config;
            }

            return null;
        }

        /// <summary>
        /// Valida o número do cartão usando o algoritmo de Luhn.
        /// </summary>
        private bool ValidarLuhn(string numero)
        {
            int soma = 0;
            bool alternar = false;

            for (int i = numero.Length - 1; i >= 0; i--)
            {
                int digito = numero[i] - '0';

                if (alternar)
                {
                    digito *= 2;
                    if (digito > 9)
                        digito -= 9;
                }

                soma += digito;
                alternar = !alternar;
            }

            return soma % 10 == 0;
        }
    }

    class Program
    {
        static void Main()
        {
            var validador = new ValidadorCartao();

            var cartoes = new[]
            {
                "4532015112830366",      
                "5425233010103442",      
                "374245455400126",       
                "36148906313152",        
                "6011111111111117",      
                "3530111333300000",      
                "8699999999999999",      
                "5078601200000000",      
                "5067123456789012",      
                "1234567890123456", 
            };

            foreach (var cartao in cartoes)
            {
                var resultado = validador.Validar(cartao);
                var status = resultado.IsValido ? "✓ VÁLIDO" : "✗ INVÁLIDO";
                Console.WriteLine($"{cartao} - {resultado.Bandeira} - {status}");
            }
        }
    }
}