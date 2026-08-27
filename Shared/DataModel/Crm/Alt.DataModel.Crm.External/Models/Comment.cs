using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataModel.Crm.External.Models
{
    public class Comment : IEquatable<Comment>
    {
        public string Title { get; set; }
        public List<string> Content { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Comment);
        }

        public bool Equals(Comment other)
        {
            if (other == null)
                return false;
            var thisNotOther = this.Content.Except(other.Content).ToList();
            var otherNotThis = other.Content.Except(this.Content).ToList();

            return this.Title.Equals(other.Title) &&
                (
                    object.ReferenceEquals(this.Title, other.Title) ||
                    this.Title != null &&
                    this.Title.Equals(other.Title)
                ) &&
                (
                    object.ReferenceEquals(this.Content, other.Content) ||
                    this.Content != null &&
                    !thisNotOther.Any() && !otherNotThis.Any()
                );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
