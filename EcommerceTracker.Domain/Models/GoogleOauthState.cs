// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OauthState.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the OauthState type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EcommerceTracker.Domain.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Google OAuth state used during authorization code flow.
    /// </summary>
    public class GoogleOauthState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string UserId { get; set; }

        public string Value { get; set; }
    }
}