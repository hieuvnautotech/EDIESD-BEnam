﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ESD_EDI_BE.Models;

[PrimaryKey("userId", "roleId")]
public partial class sysTbl_User_Role
{
    [Key]
    public long userId { get; set; }

    [Key]
    public long roleId { get; set; }

    public bool? isActived { get; set; }

    [Precision(0)]
    public DateTime? createdDate { get; set; }

    public long? createdBy { get; set; }

    [Precision(0)]
    public DateTime? modifiedDate { get; set; }

    public long? modifiedBy { get; set; }

    public byte[] row_version { get; set; }

    [ForeignKey("roleId")]
    [InverseProperty("sysTbl_User_Role")]
    public virtual sysTbl_Role role { get; set; }

    [ForeignKey("userId")]
    [InverseProperty("sysTbl_User_Role")]
    public virtual sysTbl_User user { get; set; }
}